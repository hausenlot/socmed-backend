using System.ComponentModel.DataAnnotations;

namespace socmed_backend.Models;

public class Notification
{
    [Key]
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty; // User receiving notification

    // E.g., Follow, Like, ReRant, Reply, Mention
    public string Type { get; set; } = string.Empty; 
    
    public string Message { get; set; } = string.Empty;
    public int? RelatedEntityId { get; set; }
    public string? SourceUsername { get; set; }

    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
}
