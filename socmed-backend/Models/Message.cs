using System;

namespace socmed_backend.Models;

public class Message
{
    public int Id { get; set; }
    
    public string ConversationId { get; set; } = null!;
    public Conversation Conversation { get; set; } = null!;
    
    public string SenderId { get; set; } = null!;
    public User Sender { get; set; } = null!;
    
    public string EncryptedContent { get; set; } = null!;
    public string EncryptionIv { get; set; } = null!;
    
    // Extensible slot for multimedia files
    public string? MediaId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
