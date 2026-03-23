using Microsoft.EntityFrameworkCore;
using socmed_backend.Data;
using socmed_backend.DTOs;
using socmed_backend.Models;

namespace socmed_backend.Services;

public class ReplyService : IReplyService
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IMultimediaService _multimediaService;

    public ReplyService(AppDbContext context, INotificationService notificationService, IMultimediaService multimediaService)
    {
        _context = context;
        _notificationService = notificationService;
        _multimediaService = multimediaService;
    }

    public async Task<IEnumerable<ReplyResponseDto>> GetRepliesAsync(string rantId, string? requestingUserId = null, int page = 1, int pageSize = 10)
    {
        var rant = await _context.Rants.FirstOrDefaultAsync(r => r.PublicId == rantId);
        if (rant == null) return Enumerable.Empty<ReplyResponseDto>();

        var replies = await _context.RantReplies
            .Include(r => r.User)
            .Include(r => r.Rant)
            .Where(r => r.RantId == rant.Id && !r.IsDeleted)
            .OrderBy(r => r.CreatedAt)
            .ThenBy(r => r.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return await MapToResponseDtosAsync(replies, requestingUserId);
    }

    public async Task<ReplyResponseDto?> CreateReplyAsync(string rantId, string userId, CreateReplyDto dto, string? mediaId = null, string? mediaType = null)
    {
        var rant = await _context.Rants.FirstOrDefaultAsync(r => r.PublicId == rantId);
        if (rant == null) return null;

        int? internalParentReplyId = null;
        if (!string.IsNullOrEmpty(dto.ParentReplyId))
        {
            var pr = await _context.RantReplies.FirstOrDefaultAsync(r => r.PublicId == dto.ParentReplyId);
            internalParentReplyId = pr?.Id;
        }

        var reply = new RantReply
        {
            PublicId = Guid.NewGuid().ToString(),
            RantId = rant.Id,
            UserId = userId,
            Content = dto.Content,
            ParentReplyId = internalParentReplyId,
            MediaId = mediaId,
            MediaType = mediaType
        };

        _context.RantReplies.Add(reply);
        await _context.SaveChangesAsync();
 
        var currentUser = await _context.Users.FindAsync(userId);
        var currentUsername = currentUser?.Username ?? "Someone";

        var excludeUserIds = new List<string>();

        // Notify the rant author (unless replying to own rant)
        if (rant.UserId != userId)
        {
            await _notificationService.CreateNotificationAsync(
                rant.UserId, 
                "Reply", 
                $"{currentUsername} replied to your rant.",
                currentUsername,
                rant.Id);
            excludeUserIds.Add(rant.UserId);
        }

        // If replying to another reply, also notify that reply's author
        if (internalParentReplyId.HasValue)
        {
            var parentReply = await _context.RantReplies.FindAsync(internalParentReplyId.Value);
            if (parentReply != null && parentReply.UserId != userId && !excludeUserIds.Contains(parentReply.UserId))
            {
                await _notificationService.CreateNotificationAsync(
                    parentReply.UserId,
                    "Reply",
                    $"{currentUsername} replied to your reply.",
                    currentUsername,
                    rant.Id);
                excludeUserIds.Add(parentReply.UserId);
            }
        }

        // Process mentions, excluding those already notified via Reply alerts
        await _notificationService.ProcessMentionsAsync(dto.Content, userId, rant.Id, excludeUserIds);

        // Re-fetch with User included
        var saved = await _context.RantReplies
            .Include(r => r.User)
            .FirstAsync(r => r.Id == reply.Id);

        return (await MapToResponseDtosAsync(new[] { saved }, userId)).First();
    }

    public async Task<ReplyResponseDto?> UpdateReplyAsync(string replyId, string userId, UpdateReplyDto dto)
    {
        var reply = await _context.RantReplies
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.PublicId == replyId);
        
        if (reply == null || reply.UserId != userId) return null;

        reply.Content = dto.Content;
        reply.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (await MapToResponseDtosAsync(new[] { reply }, userId)).First();
    }

    public async Task<bool> DeleteReplyAsync(string replyId, string userId)
    {
        var reply = await _context.RantReplies.FirstOrDefaultAsync(r => r.PublicId == replyId);
        
        if (reply == null || reply.UserId != userId) return false;

        reply.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleLikeAsync(string replyId, string userId)
    {
        var reply = await _context.RantReplies.FirstOrDefaultAsync(r => r.PublicId == replyId);
        if (reply == null) return false;

        var existing = await _context.ReplyLikes
            .FirstOrDefaultAsync(l => l.ReplyId == reply.Id && l.UserId == userId);

        if (existing != null)
        {
            _context.ReplyLikes.Remove(existing);
        }
        else
        {
            _context.ReplyLikes.Add(new ReplyLike { ReplyId = reply.Id, UserId = userId });
            
            // Optional: Notify the reply author about the like
            var liker = await _context.Users.FindAsync(userId);
            if (liker != null && reply.UserId != userId)
            {
                await _notificationService.CreateNotificationAsync(
                    reply.UserId,
                    "Like",
                    $"{liker.DisplayName} liked your reply.",
                    liker.Username,
                    reply.RantId
                );
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    // ── Shared helper ──────────────────────────────────────────────────────────

    private async Task<IEnumerable<ReplyResponseDto>> MapToResponseDtosAsync(
        IEnumerable<RantReply> replies, string? requestingUserId)
    {
        var replyList = replies.ToList();
        if (replyList.Count == 0) return Enumerable.Empty<ReplyResponseDto>();

        var replyIds = replyList.Select(r => r.Id).ToList();

        // Batch-load parent reply public IDs and usernames
        var parentIds = replyList
            .Where(r => r.ParentReplyId.HasValue)
            .Select(r => r.ParentReplyId!.Value)
            .Distinct()
            .ToList();

        var parentInfo = parentIds.Count > 0
            ? await _context.RantReplies
                .Include(r => r.User)
                .Where(r => parentIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, r => (dynamic)new { r.PublicId, r.User.Username })
            : new Dictionary<int, dynamic>();

        // Load like counts
        var likeCounts = await _context.ReplyLikes
            .Where(l => replyIds.Contains(l.ReplyId))
            .GroupBy(l => l.ReplyId)
            .Select(g => new { ReplyId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ReplyId, x => x.Count);

        // Load if liked by me
        var likedByMe = requestingUserId != null
            ? await _context.ReplyLikes
                .Where(l => l.UserId == requestingUserId && replyIds.Contains(l.ReplyId))
                .Select(l => l.ReplyId)
                .ToListAsync()
            : new List<int>();

        return replyList.Select(r => new ReplyResponseDto
        {
            Id = r.PublicId,
            RantId = r.Rant?.PublicId ?? string.Empty,
            Content = r.Content,
            CreatedAt = r.CreatedAt,
            UserId = r.UserId,
            Username = r.User.Username,
            DisplayName = r.User.DisplayName,
            ProfileImageUrl = r.User.ProfileMediaId != null ? _multimediaService.GetPublicUrl(r.User.ProfileMediaId) : null,
            ParentReplyId = r.ParentReplyId.HasValue && parentInfo.ContainsKey(r.ParentReplyId.Value)
                ? parentInfo[r.ParentReplyId.Value].PublicId
                : null,
            ParentReplyUsername = r.ParentReplyId.HasValue && parentInfo.ContainsKey(r.ParentReplyId.Value)
                ? parentInfo[r.ParentReplyId.Value].Username
                : null,
            LikeCount = likeCounts.ContainsKey(r.Id) ? likeCounts[r.Id] : 0,
            ReplyCount = 0, // Flattening for now
            IsLikedByMe = likedByMe.Contains(r.Id),
            MediaUrl = r.MediaId != null ? _multimediaService.GetPublicUrl(r.MediaId) : null,
            MediaType = r.MediaType
        }).ToList();
    }
}
