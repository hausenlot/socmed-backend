using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace socmed_backend.DTOs;

public class Start1To1ChatRequestDto
{
    [Required]
    public string RecipientUsername { get; set; } = string.Empty;
}

public class SendMessageRequestDto
{
    [Required]
    [StringLength(2000)]
    public string Content { get; set; } = string.Empty;
}

public class MessageResponseDto
{
    public int Id { get; set; }
    public string ConversationId { get; set; } = string.Empty;
    public string SenderUsername { get; set; } = string.Empty;
    public string SenderDisplayName { get; set; } = string.Empty;
    public string? SenderProfileImageUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? MediaId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ParticipantDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
}

public class ConversationDto
{
    public string ConversationId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public bool IsGroup { get; set; }
    
    // Details of the other participant (convenience for 1-to-1 chats)
    public ParticipantDto? OtherParticipant { get; set; }
    
    public string? LastMessageContent { get; set; }
    public DateTime? LastMessageCreatedAt { get; set; }
    public string? LastMessageSenderUsername { get; set; }
    
    public int UnreadCount { get; set; }
    public IEnumerable<ParticipantDto> Participants { get; set; } = new List<ParticipantDto>();
}
