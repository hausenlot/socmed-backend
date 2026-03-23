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

        // Define the queries as IQueryable to allow server-side paging.
        // We project into a shape that includes the Rant and its User.
        var ownRantsQuery = _context.Rants
            .Where(r => followingIds.Contains(r.UserId) && !r.IsDeleted)
            .Select(r => new { Rant = r, User = r.User, ReRantedBy = (string?)null });

        var rerantedRantsQuery = _context.RantReRants
            .Where(rr => followingIds.Contains(rr.UserId) && !rr.Rant.IsDeleted)
            .Select(rr => new { Rant = rr.Rant, User = rr.Rant.User, ReRantedBy = (string?)rr.User.Username });

        // Union the queries, Sort, and Page BEFORE calling ToListAsync
        var combinedAndPaged = await ownRantsQuery
            .Union(rerantedRantsQuery)
            .OrderByDescending(x => x.Rant.CreatedAt)
            .ThenByDescending(x => x.Rant.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Re-attach the User objects to the Rants since the Union projection might disconnect them
        var rantsToMap = combinedAndPaged.Select(x => 
        {
            x.Rant.User = x.User;
            return x.Rant;
        }).ToList();

        var dtos = (await _rantService.MapToResponseDtosAsync(rantsToMap, userId)).ToList();

        for (int i = 0; i < dtos.Count; i++)
        {
            dtos[i].ReRantedByUsername = combinedAndPaged[i].ReRantedBy;
        }

        return dtos;
    }

    public async Task<IEnumerable<RantResponseDto>> GetUserTimelineAsync(
        string username, int page = 1, int pageSize = 20, string? requestingUserId = null)
    {
        var targetUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

        if (targetUser == null) return new List<RantResponseDto>();

        var ownRantsQuery = _context.Rants
            .Where(r => r.UserId == targetUser.Id && !r.IsDeleted)
            .Select(r => new { Rant = r, User = r.User, ReRantedBy = (string?)null });

        var rerantedRantsQuery = _context.RantReRants
            .Where(rr => rr.UserId == targetUser.Id && !rr.Rant.IsDeleted)
            .Select(rr => new { Rant = rr.Rant, User = rr.Rant.User, ReRantedBy = (string?)targetUser.Username });

        var combinedAndPaged = await ownRantsQuery
            .Union(rerantedRantsQuery)
            .OrderByDescending(x => x.Rant.CreatedAt)
            .ThenByDescending(x => x.Rant.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var rantsToMap = combinedAndPaged.Select(x => 
        {
            x.Rant.User = x.User;
            return x.Rant;
        }).ToList();

        var dtos = (await _rantService.MapToResponseDtosAsync(rantsToMap, requestingUserId)).ToList();

        for (int i = 0; i < dtos.Count; i++)
        {
            dtos[i].ReRantedByUsername = combinedAndPaged[i].ReRantedBy;
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
