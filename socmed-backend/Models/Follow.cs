using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace socmed_backend.Models;

public class Follow
{
    public string FollowerId { get; set; } = string.Empty;
    public string FollowingId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User Follower { get; set; } = null!;
    public User Following { get; set; } = null!;
}
