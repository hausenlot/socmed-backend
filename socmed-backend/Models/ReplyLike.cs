using System;

namespace socmed_backend.Models;

public class ReplyLike
{
    public string UserId { get; set; } = string.Empty;
    public int ReplyId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public RantReply Reply { get; set; } = null!;
}
