namespace socmed_backend.Services;

public interface IRantInteractionService
{
    Task<bool> ToggleLikeAsync(int rantId, string userId);
    Task<bool> ToggleReRantAsync(int rantId, string userId);
    Task<bool> ToggleBookmarkAsync(int rantId, string userId);
    
    Task<IEnumerable<string>> GetLikesAsync(int rantId);
    Task<IEnumerable<string>> GetReRantsAsync(int rantId);
}
