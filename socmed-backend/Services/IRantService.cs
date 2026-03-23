using socmed_backend.DTOs;
using socmed_backend.Models;

namespace socmed_backend.Services;

public interface IRantService
{
    Task<IEnumerable<RantResponseDto>> GetAllRantsAsync(string? requestingUserId = null, int page = 1, int pageSize = 20);
    Task<RantResponseDto?> GetRantByIdAsync(string id, string? requestingUserId = null);
    Task<RantResponseDto> CreateRantAsync(string content, string userId, string? quoteRantId = null, string? mediaId = null, string? mediaType = null);
    Task<bool> UpdateRantAsync(string id, string content, string userId);
    Task<bool> SoftDeleteRantAsync(string id, string userId);
    Task<IEnumerable<RantResponseDto>> GetExploreRantsAsync(string? requestingUserId = null, int page = 1, int pageSize = 20);
    Task<IEnumerable<RantResponseDto>> MapToResponseDtosAsync(IEnumerable<Rant> rants, string? requestingUserId);
}
