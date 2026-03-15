using System.ComponentModel.DataAnnotations;

namespace socmed_backend.Models;

public class RantReply
{
    [Key]
    public int Id { get; set; }

    public int RantId { get; set; }
    public string UserId { get; set; } = string.Empty;

    /// <summary>Optional: the reply this is responding to (flat threading).</summary>
    public int? ParentReplyId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Support soft-delete for replies as well
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public Rant Rant { get; set; } = null!;
    public User User { get; set; } = null!;
}
