namespace socmed_backend.Services;

public interface IRantInteractionService
{
    Task<bool> ToggleLikeAsync(string rantId, string userId);
    Task<bool> ToggleReRantAsync(string rantId, string userId);
    Task<bool> ToggleBookmarkAsync(string rantId, string userId);
    
    Task<IEnumerable<string>> GetLikesAsync(string rantId);
    Task<IEnumerable<string>> GetReRantsAsync(string rantId);
}
