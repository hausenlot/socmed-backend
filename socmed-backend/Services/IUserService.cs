using socmed_backend.DTOs;
using socmed_backend.Models;

namespace socmed_backend.Services;

public interface IUserService
{
    Task<UserProfileDto?> GetUserByUsernameAsync(string username, string? requestingUserId = null);
    Task<IEnumerable<RantResponseDto>> GetUserRantsAsync(string username, int page = 1, int pageSize = 10, string? requestingUserId = null);
    Task<IEnumerable<ReplyResponseDto>> GetUserRepliesAsync(string username, int page = 1, int pageSize = 10);
    Task<IEnumerable<RantResponseDto>> GetUserLikedRantsAsync(string username, int page = 1, int pageSize = 10, string? requestingUserId = null);
    Task<IEnumerable<User>> SearchUsersAsync(string query);
    Task<IEnumerable<UserProfileDto>> GetSuggestedUsersAsync(string? requestingUserId, int count = 5);

    Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto);
    Task<bool> UpdateProfileImageAsync(string userId, IFormFile file);
    Task<bool> RemoveProfileImageAsync(string userId);
    Task<bool> UpdateBannerImageAsync(string userId, IFormFile file);
    Task<bool> RemoveBannerImageAsync(string userId);
}
