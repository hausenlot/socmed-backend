using System.ComponentModel.DataAnnotations;

namespace socmed_backend.Models;

public class User
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    // Required unique username for login/profile
    public string Username { get; set; } = string.Empty;

    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? ProfileImageUrl { get; set; }

    // Auth fields — populated on register, null for seeded test users
    public string? PasswordHash { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<Rant> Rants { get; set; } = new List<Rant>();
    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<RantLike> Likes { get; set; } = new List<RantLike>();
    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<RantReRant> ReRants { get; set; } = new List<RantReRant>();
    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<RantBookmark> Bookmarks { get; set; } = new List<RantBookmark>();
    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<RantReply> Replies { get; set; } = new List<RantReply>();
    
    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<Follow> Followers { get; set; } = new List<Follow>();
    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<Follow> Following { get; set; } = new List<Follow>();
    
    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
