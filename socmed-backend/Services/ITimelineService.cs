using socmed_backend.DTOs;

namespace socmed_backend.Services;

public interface ITimelineService
{
    Task<IEnumerable<RantResponseDto>> GetHomeTimelineAsync(string userId, int page = 1, int pageSize = 20);
    Task<IEnumerable<RantResponseDto>> GetUserTimelineAsync(string username, int page = 1, int pageSize = 20, string? requestingUserId = null);
    Task<IEnumerable<RantResponseDto>> GetMentionsTimelineAsync(string username, int page = 1, int pageSize = 20, string? requestingUserId = null);
    Task<IEnumerable<RantResponseDto>> GetBookmarksTimelineAsync(string userId, int page = 1, int pageSize = 20);
}
