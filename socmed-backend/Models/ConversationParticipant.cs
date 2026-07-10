using System;

namespace socmed_backend.Models;

public class ConversationParticipant
{
    public string ConversationId { get; set; } = null!;
    public Conversation Conversation { get; set; } = null!;

    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastReadAt { get; set; } = DateTime.UtcNow;
}
