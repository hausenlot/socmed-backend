using System.ComponentModel.DataAnnotations;

namespace socmed_backend.DTOs;

public class CreateReplyDto
{
    [Required]
    [MaxLength(280)]
    public string Content { get; set; } = string.Empty;

    /// <summary>Optional: ID of the reply this is responding to.</summary>
    public int? ParentReplyId { get; set; }

    public Microsoft.AspNetCore.Http.IFormFile? MediaFile { get; set; }
}

public class UpdateReplyDto
{
    [Required]
    [MaxLength(280)]
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Enriched reply response returned to the frontend.
/// </summary>
public class ReplyResponseDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Author info
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? ProfileImageUrl { get; set; }

    // Parent reply reference (flat threading)
    public int? ParentReplyId { get; set; }
    public string? ParentReplyUsername { get; set; }

    // Counts
    public int LikeCount { get; set; }
    public int ReplyCount { get; set; }

    // Per-user flag
    public bool IsLikedByMe { get; set; }

    // Multimedia
    public string? MediaUrl { get; set; }
    public string? MediaType { get; set; }
}
