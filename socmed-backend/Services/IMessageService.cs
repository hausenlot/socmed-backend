using System.Collections.Generic;
using System.Threading.Tasks;
using socmed_backend.DTOs;

namespace socmed_backend.Services;

public interface IMessageService
{
    Task<ConversationDto> GetOrCreate1To1ConversationAsync(string currentUserId, string otherUsername);
    Task<IEnumerable<ConversationDto>> GetConversationsAsync(string currentUserId);
    Task<IEnumerable<MessageResponseDto>> GetChatHistoryAsync(string currentUserId, string conversationId, int page = 1, int pageSize = 50);
    Task<MessageResponseDto?> SendMessageAsync(string currentUserId, string conversationId, string content);
    Task<bool> MarkAsReadAsync(string currentUserId, string conversationId);
}
