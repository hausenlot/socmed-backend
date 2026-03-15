using Microsoft.EntityFrameworkCore;
using socmed_backend.Data;
using socmed_backend.DTOs;
using socmed_backend.Models;

namespace socmed_backend.Services;

public class ReplyService : IReplyService
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;

    public ReplyService(AppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<ReplyResponseDto>> GetRepliesAsync(int rantId, string? requestingUserId = null, int page = 1, int pageSize = 10)
    {
        var replies = await _context.RantReplies
            .Include(r => r.User)
            .Where(r => r.RantId == rantId && !r.IsDeleted)
            .OrderBy(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return await MapToResponseDtosAsync(replies, requestingUserId);
    }

    public async Task<ReplyResponseDto?> CreateReplyAsync(int rantId, string userId, CreateReplyDto dto)
    {
        var rant = await _context.Rants.FindAsync(rantId);
        if (rant == null) return null;

        var reply = new RantReply
        {
            RantId = rantId,
            UserId = userId,
            Content = dto.Content,
            ParentReplyId = dto.ParentReplyId
        };

        _context.RantReplies.Add(reply);
        await _context.SaveChangesAsync();

        var currentUser = await _context.Users.FindAsync(userId);
        var currentUsername = currentUser?.Username ?? "Someone";

        // Notify the rant author (unless replying to own rant)
        if (rant.UserId != userId)
        {
            await _notificationService.CreateNotificationAsync(
                rant.UserId, 
                "Reply", 
                $"{currentUsername} replied to your rant.",
                currentUsername,
                rantId);
        }

        // If replying to another reply, also notify that reply's author
        if (dto.ParentReplyId.HasValue)
        {
            var parentReply = await _context.RantReplies.FindAsync(dto.ParentReplyId.Value);
            if (parentReply != null && parentReply.UserId != userId && parentReply.UserId != rant.UserId)
            {
                await _notificationService.CreateNotificationAsync(
                    parentReply.UserId,
                    "Reply",
                    $"{currentUsername} replied to your reply.",
                    currentUsername,
                    rantId);
            }
        }

        // Re-fetch with User included
        var saved = await _context.RantReplies
            .Include(r => r.User)
            .FirstAsync(r => r.Id == reply.Id);

        return (await MapToResponseDtosAsync(new[] { saved }, userId)).First();
    }

    public async Task<ReplyResponseDto?> UpdateReplyAsync(int replyId, string userId, UpdateReplyDto dto)
    {
        var reply = await _context.RantReplies
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == replyId);
        
        if (reply == null || reply.UserId != userId) return null;

        reply.Content = dto.Content;
        reply.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (await MapToResponseDtosAsync(new[] { reply }, userId)).First();
    }

    public async Task<bool> DeleteReplyAsync(int replyId, string userId)
    {
        var reply = await _context.RantReplies.FindAsync(replyId);
        
        if (reply == null || reply.UserId != userId) return false;

        reply.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    // ── Shared helper ──────────────────────────────────────────────────────────

    private async Task<IEnumerable<ReplyResponseDto>> MapToResponseDtosAsync(
        IEnumerable<RantReply> replies, string? requestingUserId)
    {
        var replyList = replies.ToList();

        // Batch-load parent reply usernames
        var parentIds = replyList
            .Where(r => r.ParentReplyId.HasValue)
            .Select(r => r.ParentReplyId!.Value)
            .Distinct()
            .ToList();

        var parentUsernames = parentIds.Count > 0
            ? await _context.RantReplies
                .Include(r => r.User)
                .Where(r => parentIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, r => r.User.Username)
            : new Dictionary<int, string>();

        return replyList.Select(r => new ReplyResponseDto
        {
            Id = r.Id,
            Content = r.Content,
            CreatedAt = r.CreatedAt,
            UserId = r.UserId,
            Username = r.User.Username,
            DisplayName = r.User.DisplayName,
            ProfileImageUrl = r.User.ProfileImageUrl,
            ParentReplyId = r.ParentReplyId,
            ParentReplyUsername = r.ParentReplyId.HasValue && parentUsernames.ContainsKey(r.ParentReplyId.Value)
                ? parentUsernames[r.ParentReplyId.Value]
                : null,
            LikeCount = 0,
            ReplyCount = 0,
            IsLikedByMe = false
        }).ToList();
    }
}
