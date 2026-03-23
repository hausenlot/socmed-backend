using Microsoft.EntityFrameworkCore;
using socmed_backend.Data;
using socmed_backend.DTOs;
using socmed_backend.Models;

namespace socmed_backend.Services;

public class RantService : IRantService
{
    private readonly AppDbContext _context;
    private readonly IMultimediaService _multimediaService;
    private readonly INotificationService _notificationService;
 
    public RantService(AppDbContext context, IMultimediaService multimediaService, INotificationService notificationService)
    {
        _context = context;
        _multimediaService = multimediaService;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<RantResponseDto>> GetAllRantsAsync(string? requestingUserId = null, int page = 1, int pageSize = 20)
    {
        var rants = await _context.Rants
            .Include(r => r.User)
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return await MapToResponseDtosAsync(rants, requestingUserId);
    }

    public async Task<RantResponseDto?> GetRantByIdAsync(string id, string? requestingUserId = null)
    {
        var rant = await _context.Rants
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.PublicId == id);

        if (rant == null) return null;
        return (await MapToResponseDtosAsync(new[] { rant }, requestingUserId)).First();
    }

    public async Task<RantResponseDto> CreateRantAsync(string content, string userId, string? quoteRantId = null, string? mediaId = null, string? mediaType = null)
    {
        int? internalQuoteRantId = null;
        if (!string.IsNullOrEmpty(quoteRantId))
        {
            var qr = await _context.Rants.FirstOrDefaultAsync(r => r.PublicId == quoteRantId);
            internalQuoteRantId = qr?.Id;
        }

        var rant = new Rant
        {
            PublicId = Guid.NewGuid().ToString(),
            Content = content,
            UserId = userId,
            QuoteRantId = internalQuoteRantId,
            MediaId = mediaId,
            MediaType = mediaType,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Rants.Add(rant);
        await _context.SaveChangesAsync();
 
        // Process mentions
        await _notificationService.ProcessMentionsAsync(content, userId, rant.Id);
 
        // Re-fetch with Include so User nav prop is populated
        var savedRant = await _context.Rants
            .Include(r => r.User)
            .FirstAsync(r => r.Id == rant.Id);

        return (await MapToResponseDtosAsync(new[] { savedRant }, userId)).First();
    }

    public async Task<bool> UpdateRantAsync(string id, string content, string userId)
    {
        var rant = await _context.Rants.FirstOrDefaultAsync(r => r.PublicId == id);
        if (rant == null || rant.UserId != userId) return false;

        rant.Content = content;
        rant.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SoftDeleteRantAsync(string id, string userId)
    {
        var rant = await _context.Rants.FirstOrDefaultAsync(r => r.PublicId == id);
        if (rant == null || rant.UserId != userId) return false;

        rant.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<RantResponseDto>> GetExploreRantsAsync(string? requestingUserId = null, int page = 1, int pageSize = 20)
    {
        var rants = await _context.Rants
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return await MapToResponseDtosAsync(rants, requestingUserId);
    }

    // ── Shared helper ──────────────────────────────────────────────────────────

    public async Task<IEnumerable<RantResponseDto>> MapToResponseDtosAsync(
        IEnumerable<Rant> rants, string? requestingUserId)
    {
        var rantIds = rants.Select(r => r.Id).ToList();

        // Batch-load aggregate counts
        var likeCounts = await _context.RantLikes
            .Where(l => rantIds.Contains(l.RantId))
            .GroupBy(l => l.RantId)
            .Select(g => new { RantId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RantId, x => x.Count);

        var replyCounts = await _context.RantReplies
            .Where(r => rantIds.Contains(r.RantId))
            .GroupBy(r => r.RantId)
            .Select(g => new { RantId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RantId, x => x.Count);

        var reRantCounts = await _context.RantReRants
            .Where(r => rantIds.Contains(r.RantId))
            .GroupBy(r => r.RantId)
            .Select(g => new { RantId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RantId, x => x.Count);

        // Batch-load per-user flags
        HashSet<int> likedByMe = new();
        HashSet<int> rerantedByMe = new();
        HashSet<int> bookmarkedByMe = new();

        if (requestingUserId != null)
        {
            likedByMe = (await _context.RantLikes
                .Where(l => rantIds.Contains(l.RantId) && l.UserId == requestingUserId)
                .Select(l => l.RantId)
                .ToListAsync()).ToHashSet();

            rerantedByMe = (await _context.RantReRants
                .Where(r => rantIds.Contains(r.RantId) && r.UserId == requestingUserId)
                .Select(r => r.RantId)
                .ToListAsync()).ToHashSet();

            bookmarkedByMe = (await _context.RantBookmarks
                .Where(b => rantIds.Contains(b.RantId) && b.UserId == requestingUserId)
                .Select(b => b.RantId)
                .ToListAsync()).ToHashSet();
        }

        // Batch-load quoted rants
        var quoteRantIds = rants
            .Where(r => r.QuoteRantId.HasValue)
            .Select(r => r.QuoteRantId!.Value)
            .Distinct()
            .ToList();

        var quoteRants = quoteRantIds.Count > 0
            ? await _context.Rants
                .Include(r => r.User)
                .Where(r => quoteRantIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id)
            : new Dictionary<int, Rant>();

        return rants.Select(r =>
        {
            QuoteRantDto? quoteRantDto = null;
            if (r.QuoteRantId.HasValue && quoteRants.TryGetValue(r.QuoteRantId.Value, out var qr))
            {
                quoteRantDto = new QuoteRantDto
                {
                    Id = qr.PublicId,
                    Content = qr.Content,
                    Username = qr.User.Username,
                    DisplayName = qr.User.DisplayName,
                    CreatedAt = qr.CreatedAt,
                    ProfileImageUrl = qr.User.ProfileMediaId != null ? _multimediaService.GetPublicUrl(qr.User.ProfileMediaId) : null,
                    MediaUrl = qr.MediaId != null ? _multimediaService.GetPublicUrl(qr.MediaId) : null,
                    MediaType = qr.MediaType
                };
            }

            return new RantResponseDto
            {
                Id = r.PublicId,
                Content = r.Content,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                UserId = r.UserId,
                Username = r.User.Username,
                DisplayName = r.User.DisplayName,
                ProfileImageUrl = r.User.ProfileMediaId != null ? _multimediaService.GetPublicUrl(r.User.ProfileMediaId) : null,
                LikeCount = likeCounts.GetValueOrDefault(r.Id, 0),
                ReplyCount = replyCounts.GetValueOrDefault(r.Id, 0),
                ReRantCount = reRantCounts.GetValueOrDefault(r.Id, 0),
                IsLikedByMe = likedByMe.Contains(r.Id),
                IsRerantedByMe = rerantedByMe.Contains(r.Id),
                IsBookmarkedByMe = bookmarkedByMe.Contains(r.Id),
                QuoteRantId = quoteRantDto?.Id,
                QuoteRant = quoteRantDto,
                MediaUrl = r.MediaId != null ? _multimediaService.GetPublicUrl(r.MediaId) : null,
                MediaType = r.MediaType
            };
        });
    }
}
