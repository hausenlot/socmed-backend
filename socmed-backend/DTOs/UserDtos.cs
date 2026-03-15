using System.ComponentModel.DataAnnotations;

namespace socmed_backend.DTOs;

public class UserProfileDto
{
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    // Social stats
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public int RantCount { get; set; }

    // Per-viewer flag (false if not authenticated or viewing own profile)
    public bool IsFollowedByMe { get; set; }
}

public class UpdateProfileDto
{
    [StringLength(50)]
    public string? DisplayName { get; set; }

    [StringLength(160)]
    public string? Bio { get; set; }
}
