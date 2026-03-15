namespace socmed_backend.Models;

public class RantBookmark
{
    public string UserId { get; set; } = string.Empty;
    public int RantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Rant Rant { get; set; } = null!;
}
