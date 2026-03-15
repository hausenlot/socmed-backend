using socmed_backend.DTOs;

namespace socmed_backend.Services;

public interface IReplyService
{
    Task<IEnumerable<ReplyResponseDto>> GetRepliesAsync(int rantId, string? requestingUserId = null, int page = 1, int pageSize = 10);
    Task<ReplyResponseDto?> CreateReplyAsync(int rantId, string userId, CreateReplyDto dto);
    Task<ReplyResponseDto?> UpdateReplyAsync(int replyId, string userId, UpdateReplyDto dto);
    Task<bool> DeleteReplyAsync(int replyId, string userId);
}
