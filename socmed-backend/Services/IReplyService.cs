using socmed_backend.DTOs;

namespace socmed_backend.Services;

public interface IReplyService
{
    Task<IEnumerable<ReplyResponseDto>> GetRepliesAsync(string rantId, string? requestingUserId = null, int page = 1, int pageSize = 10);
    Task<ReplyResponseDto?> CreateReplyAsync(string rantId, string userId, CreateReplyDto dto, string? mediaId = null, string? mediaType = null);
    Task<ReplyResponseDto?> UpdateReplyAsync(string replyId, string userId, UpdateReplyDto dto);
    Task<bool> DeleteReplyAsync(string replyId, string userId);
    Task<bool> ToggleLikeAsync(string replyId, string userId);
}
