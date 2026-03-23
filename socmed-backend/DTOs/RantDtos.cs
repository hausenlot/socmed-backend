using System.ComponentModel.DataAnnotations;

namespace socmed_backend.DTOs;

public class CreateRantDto
{
    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string Content { get; set; } = string.Empty;

    /// <summary>Optional: ID of the rant being quoted (quote re-rant).</summary>
    public string? QuoteRantId { get; set; }

    /// <summary>Optional: multimedia file to attach to the rant.</summary>
    public IFormFile? MediaFile { get; set; }
}

public class UpdateRantDto
{
    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// The standard rant response returned to the frontend.
/// Flat structure with author info, aggregate counts, and per-user interaction flags.
/// </summary>
public class RantResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Author info (flat — matches frontend RantDto)
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? ProfileImageUrl { get; set; }

    // Aggregate counts
    public int LikeCount { get; set; }
    public int ReplyCount { get; set; }
    public int ReRantCount { get; set; }

    // Flags for the currently authenticated user (false if not authenticated)
    public bool IsLikedByMe { get; set; }
    public bool IsRerantedByMe { get; set; }
    public bool IsBookmarkedByMe { get; set; }

    // Re-rant indicator (set when this rant appears because someone re-ranted it)
    public string? ReRantedByUsername { get; set; }

    // Quote re-rant (embedded original rant)
    public string? QuoteRantId { get; set; }
    public QuoteRantDto? QuoteRant { get; set; }

    // Multimedia support
    public string? MediaUrl { get; set; }
    public string? MediaType { get; set; }
}

/// <summary>
/// Lightweight rant info embedded inside a quote re-rant.
/// </summary>
public class QuoteRantDto
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? MediaUrl { get; set; }
    public string? MediaType { get; set; }
}
