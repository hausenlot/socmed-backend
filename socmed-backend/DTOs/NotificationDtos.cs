namespace socmed_backend.DTOs;

/// <summary>
/// Notification response returned to the frontend.
/// Maps RelatedEntityId → RantId for frontend convenience.
/// </summary>
public class NotificationResponseDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? SourceUsername { get; set; }
    public int? RantId { get; set; }
}
