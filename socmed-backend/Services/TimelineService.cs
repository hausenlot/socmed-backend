using Microsoft.EntityFrameworkCore;
using socmed_backend.Data;
using socmed_backend.DTOs;
using socmed_backend.Models;

namespace socmed_backend.Services;

public class TimelineService : ITimelineService
{
    private readonly AppDbContext _context;
    private readonly IRantService _rantService;

    public TimelineService(AppDbContext context, IRantService rantService)
    {
        _context = context;
        _rantService = rantService;
    }

    public async Task<IEnumerable<RantResponseDto>> GetHomeTimelineAsync(string userId, int page = 1, int pageSize = 20)
    {
        var followingIds = await _context.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        followingIds.Add(userId);

        // Own rants (reRantedBy = null)
        var ownRants = await _context.Rants
            .Include(r => r.User)
            .Where(r => followingIds.Contains(r.UserId) && !r.IsDeleted)
            .Select(r => new { Rant = r, ReRantedBy = (string?)null })
            .ToListAsync();

        // Rants re-ranted by followed users
        var rerantedRants = await _context.RantReRants
            .Include(rr => rr.Rant)
                .ThenInclude(r => r.User)
            .Include(rr => rr.User) // The person who re-ranted
            .Where(rr => followingIds.Contains(rr.UserId) && !rr.Rant.IsDeleted)
            .Select(rr => new { Rant = rr.Rant, ReRantedBy = (string?)rr.User.Username })
            .ToListAsync();

        var combinedAndSorted = ownRants.Concat(rerantedRants)
            .OrderByDescending(x => x.Rant.CreatedAt)
            .ThenByDescending(x => x.Rant.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var rantsToMap = combinedAndSorted.Select(x => x.Rant).ToList();
        var dtos = (await _rantService.MapToResponseDtosAsync(rantsToMap, userId)).ToList();

        for (int i = 0; i < dtos.Count; i++)
        {
            dtos[i].ReRantedByUsername = combinedAndSorted[i].ReRantedBy;
        }

        return dtos;
    }

    public async Task<IEnumerable<RantResponseDto>> GetUserTimelineAsync(
        string username, int page = 1, int pageSize = 20, string? requestingUserId = null)
    {
        var targetUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

        if (targetUser == null) return new List<RantResponseDto>();

        var ownRants = await _context.Rants
            .Include(r => r.User)
            .Where(r => r.UserId == targetUser.Id && !r.IsDeleted)
            .Select(r => new { Rant = r, ReRantedBy = (string?)null })
            .ToListAsync();

        var rerantedRants = await _context.RantReRants
            .Include(rr => rr.Rant)
                .ThenInclude(r => r.User)
            .Where(rr => rr.UserId == targetUser.Id && !rr.Rant.IsDeleted)
            .Select(rr => new { Rant = rr.Rant, ReRantedBy = (string?)targetUser.Username })
            .ToListAsync();

        var combinedAndSorted = ownRants.Concat(rerantedRants)
            .OrderByDescending(x => x.Rant.CreatedAt)
            .ThenByDescending(x => x.Rant.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var rantsToMap = combinedAndSorted.Select(x => x.Rant).ToList();
        var dtos = (await _rantService.MapToResponseDtosAsync(rantsToMap, requestingUserId)).ToList();

        for (int i = 0; i < dtos.Count; i++)
        {
            dtos[i].ReRantedByUsername = combinedAndSorted[i].ReRantedBy;
        }

        return dtos;
    }

    public async Task<IEnumerable<RantResponseDto>> GetMentionsTimelineAsync(
        string username, int page = 1, int pageSize = 20, string? requestingUserId = null)
    {
        string mentionString = $"@{username.ToLower()}";

        var rants = await _context.Rants
            .Include(r => r.User)
            .Where(r => r.Content.ToLower().Contains(mentionString) && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return await _rantService.MapToResponseDtosAsync(rants, requestingUserId);
    }

    public async Task<IEnumerable<RantResponseDto>> GetBookmarksTimelineAsync(
        string userId, int page = 1, int pageSize = 20)
    {
        var bookmarkedRants = await _context.RantBookmarks
            .Include(b => b.Rant)
                .ThenInclude(r => r.User)
            .Where(b => b.UserId == userId && !b.Rant.IsDeleted)
            .OrderByDescending(b => b.CreatedAt)
            .ThenByDescending(b => b.Rant.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var rants = bookmarkedRants.Select(b => b.Rant).ToList();

        return await _rantService.MapToResponseDtosAsync(rants, userId);
    }
}
