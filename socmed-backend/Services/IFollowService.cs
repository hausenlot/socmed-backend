using socmed_backend.DTOs;

namespace socmed_backend.Services;

public interface IFollowService
{
    Task<bool> ToggleFollowAsync(string followerId, string followingUsername);
    Task<IEnumerable<UserProfileDto>> GetFollowersAsync(string username, int page = 1, int pageSize = 20, string? requestingUserId = null);
    Task<IEnumerable<UserProfileDto>> GetFollowingAsync(string username, int page = 1, int pageSize = 20, string? requestingUserId = null);
}
