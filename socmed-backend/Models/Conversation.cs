using System;
using System.Collections.Generic;

namespace socmed_backend.Models;

public class Conversation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // Optional name for group chats (null for DMs)
    public string? Name { get; set; }
    
    public bool IsGroup { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
