using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using socmed_backend.Data;
using socmed_backend.DTOs;
using socmed_backend.Hubs;
using socmed_backend.Models;

namespace socmed_backend.Services;

public class MessageService : IMessageService
{
    private readonly AppDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IMultimediaService _multimediaService;

    public MessageService(
        AppDbContext context,
        IEncryptionService encryptionService,
        IHubContext<NotificationHub> hubContext,
        IMultimediaService multimediaService)
    {
        _context = context;
        _encryptionService = encryptionService;
        _hubContext = hubContext;
        _multimediaService = multimediaService;
    }

    public async Task<ConversationDto> GetOrCreate1To1ConversationAsync(string currentUserId, string otherUsername)
    {
        var otherUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == otherUsername.ToLower());
        
        if (otherUser == null)
            throw new KeyNotFoundException($"User with username '{otherUsername}' not found.");

        if (otherUser.Id == currentUserId)
            throw new InvalidOperationException("You cannot start a conversation with yourself.");

        // Look for an existing 1-to-1 conversation between these two users
        var conversation = await _context.Conversations
            .Where(c => !c.IsGroup)
            .Where(c => c.Participants.Any(p => p.UserId == currentUserId) && 
                        c.Participants.Any(p => p.UserId == otherUser.Id))
            .Include(c => c.Participants)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync();

        if (conversation == null)
        {
            conversation = new Conversation
            {
                IsGroup = false
            };

            _context.Conversations.Add(conversation);

            var participant1 = new ConversationParticipant
            {
                ConversationId = conversation.Id,
                UserId = currentUserId
            };

            var participant2 = new ConversationParticipant
            {
                ConversationId = conversation.Id,
                UserId = otherUser.Id
            };

            _context.ConversationParticipants.AddRange(participant1, participant2);
            await _context.SaveChangesAsync();

            // Reload with relations populated
            conversation = await _context.Conversations
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .FirstAsync(c => c.Id == conversation.Id);
        }

        return MapToConversationDto(conversation, currentUserId);
    }

    public async Task<IEnumerable<ConversationDto>> GetConversationsAsync(string currentUserId)
    {
        var conversations = await _context.Conversations
            .Where(c => c.Participants.Any(p => p.UserId == currentUserId))
            .Include(c => c.Participants)
                .ThenInclude(p => p.User)
            .Include(c => c.Messages)
            .ToListAsync();

        var dtoList = new List<ConversationDto>();

        foreach (var conv in conversations)
        {
            var dto = MapToConversationDto(conv, currentUserId);
            
            // Get the last message
            var lastMsg = conv.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
            if (lastMsg != null)
            {
                dto.LastMessageContent = _encryptionService.Decrypt(lastMsg.EncryptedContent, lastMsg.EncryptionIv);
                dto.LastMessageCreatedAt = lastMsg.CreatedAt;
                
                var sender = conv.Participants.FirstOrDefault(p => p.UserId == lastMsg.SenderId)?.User;
                dto.LastMessageSenderUsername = sender?.Username;
            }

            // Calculate unread count
            var currentParticipant = conv.Participants.First(p => p.UserId == currentUserId);
            dto.UnreadCount = conv.Messages
                .Count(m => m.SenderId != currentUserId && m.CreatedAt > currentParticipant.LastReadAt);

            dtoList.Add(dto);
        }

        // Return sorted by last message timestamp (or creation date if no messages)
        return dtoList
            .OrderByDescending(d => d.LastMessageCreatedAt ?? DateTime.MinValue)
            .ThenByDescending(d => d.ConversationId);
    }

    public async Task<IEnumerable<MessageResponseDto>> GetChatHistoryAsync(string currentUserId, string conversationId, int page = 1, int pageSize = 50)
    {
        // Security check: Make sure user is a participant
        var isParticipant = await _context.ConversationParticipants
            .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == currentUserId);

        if (!isParticipant)
            throw new UnauthorizedAccessException("You are not authorized to view this conversation.");

        var messages = await _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .Include(m => m.Sender)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Map and Decrypt (Newest first, but we order by CreatedAt ascending for final UI render)
        return messages
            .Select(m => MapToMessageDto(m))
            .OrderBy(m => m.CreatedAt)
            .ToList();
    }

    public async Task<MessageResponseDto?> SendMessageAsync(string currentUserId, string conversationId, string content)
    {
        // Security check: Make sure user is a participant
        var senderParticipant = await _context.ConversationParticipants
            .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == currentUserId);

        if (senderParticipant == null)
            throw new UnauthorizedAccessException("You are not a participant in this conversation.");

        // Encrypt the message content
        var (cipherText, iv) = _encryptionService.Encrypt(content);

        var message = new Message
        {
            ConversationId = conversationId,
            SenderId = currentUserId,
            EncryptedContent = cipherText,
            EncryptionIv = iv,
            CreatedAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        
        // Update sender's LastReadAt immediately since they read their own message
        senderParticipant.LastReadAt = message.CreatedAt;
        
        await _context.SaveChangesAsync();

        // Reload details for DTO mapping
        var savedMessage = await _context.Messages
            .Include(m => m.Sender)
            .FirstAsync(m => m.Id == message.Id);

        var responseDto = MapToMessageDto(savedMessage);

        // Push to other participants in real-time
        var otherParticipants = await _context.ConversationParticipants
            .Where(cp => cp.ConversationId == conversationId && cp.UserId != currentUserId)
            .Select(cp => cp.UserId)
            .ToListAsync();

        foreach (var userId in otherParticipants)
        {
            // Send standard "ReceiveMessage" SignalR push targeting that user's group
            await _hubContext.Clients.Group(userId).SendAsync("ReceiveMessage", responseDto);
        }

        return responseDto;
    }

    public async Task<bool> MarkAsReadAsync(string currentUserId, string conversationId)
    {
        var participant = await _context.ConversationParticipants
            .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == currentUserId);

        if (participant == null) return false;

        participant.LastReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    /* ------------------------------ Helper Mappers ------------------------------ */

    private ConversationDto MapToConversationDto(Conversation conversation, string currentUserId)
    {
        var participants = conversation.Participants.Select(p => new ParticipantDto
        {
            Id = p.User.Id,
            Username = p.User.Username,
            DisplayName = p.User.DisplayName ?? p.User.Username,
            ProfileImageUrl = p.User.ProfileMediaId != null ? _multimediaService.GetPublicUrl(p.User.ProfileMediaId) : null
        }).ToList();

        // Resolve other participant details for 1-to-1
        ParticipantDto? otherParticipant = null;
        if (!conversation.IsGroup)
        {
            otherParticipant = participants.FirstOrDefault(p => p.Id != currentUserId);
        }

        return new ConversationDto
        {
            ConversationId = conversation.Id,
            Name = conversation.Name,
            IsGroup = conversation.IsGroup,
            Participants = participants,
            OtherParticipant = otherParticipant
        };
    }

    private MessageResponseDto MapToMessageDto(Message m)
    {
        return new MessageResponseDto
        {
            Id = m.Id,
            ConversationId = m.ConversationId,
            SenderUsername = m.Sender.Username,
            SenderDisplayName = m.Sender.DisplayName ?? m.Sender.Username,
            SenderProfileImageUrl = m.Sender.ProfileMediaId != null ? _multimediaService.GetPublicUrl(m.Sender.ProfileMediaId) : null,
            Content = _encryptionService.Decrypt(m.EncryptedContent, m.EncryptionIv),
            MediaId = m.MediaId,
            CreatedAt = m.CreatedAt
        };
    }
}
