namespace socmed_backend.Models;

public class Rant
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public string? MediaId { get; set; }
    public string? MediaType { get; set; } // "image" or "video"

    /// <summary>Optional: the rant this is quoting (quote re-rant).</summary>
    public int? QuoteRantId { get; set; }

    public User User { get; set; } = null!;
    public Rant? QuoteRant { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<RantReply> Replies { get; set; } = new List<RantReply>();
}

