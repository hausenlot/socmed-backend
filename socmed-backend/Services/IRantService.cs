using socmed_backend.DTOs;
using socmed_backend.Models;

namespace socmed_backend.Services;

public interface IRantService
{
    Task<IEnumerable<RantResponseDto>> GetAllRantsAsync(string? requestingUserId = null);
    Task<RantResponseDto?> GetRantByIdAsync(int id, string? requestingUserId = null);
    Task<RantResponseDto> CreateRantAsync(string content, string userId, int? quoteRantId = null, string? mediaId = null, string? mediaType = null);
    Task<bool> UpdateRantAsync(int id, string content, string userId);
    Task<bool> SoftDeleteRantAsync(int id, string userId);
    Task<IEnumerable<RantResponseDto>> GetExploreRantsAsync(string? requestingUserId = null);
    Task<IEnumerable<RantResponseDto>> MapToResponseDtosAsync(IEnumerable<Rant> rants, string? requestingUserId);
}
