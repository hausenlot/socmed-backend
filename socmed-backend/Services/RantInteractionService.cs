using Microsoft.EntityFrameworkCore;
using socmed_backend.Data;
using socmed_backend.Models;

namespace socmed_backend.Services;

public class RantInteractionService : IRantInteractionService
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;

    public RantInteractionService(AppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<bool> ToggleLikeAsync(string rantId, string userId)
    {
        var rant = await _context.Rants.FirstOrDefaultAsync(r => r.PublicId == rantId);
        if (rant == null) return false;

        var existingLike = await _context.RantLikes
            .FirstOrDefaultAsync(l => l.RantId == rant.Id && l.UserId == userId);

        if (existingLike != null)
        {
            _context.RantLikes.Remove(existingLike);
            await _context.SaveChangesAsync();
        }
        else
        {
            _context.RantLikes.Add(new RantLike { RantId = rant.Id, UserId = userId });
            await _context.SaveChangesAsync();

            // Trigger notification
            if (rant.UserId != userId)
            {
                var user = await _context.Users.FindAsync(userId);
                var username = user?.Username ?? "Someone";
                await _notificationService.CreateNotificationAsync(
                    rant.UserId,
                    "Like",
                    $"{username} liked your rant.",
                    username,
                    rant.Id
                );
            }
        }

        return true;
    }

    public async Task<bool> ToggleReRantAsync(string rantId, string userId)
    {
        var rant = await _context.Rants.FirstOrDefaultAsync(r => r.PublicId == rantId);
        if (rant == null) return false;

        var existingReRant = await _context.RantReRants
            .FirstOrDefaultAsync(r => r.RantId == rant.Id && r.UserId == userId);

        if (existingReRant != null)
        {
            _context.RantReRants.Remove(existingReRant);
            await _context.SaveChangesAsync();
        }
        else
        {
            _context.RantReRants.Add(new RantReRant { RantId = rant.Id, UserId = userId });
            await _context.SaveChangesAsync();

            // Trigger notification
            if (rant.UserId != userId)
            {
                var user = await _context.Users.FindAsync(userId);
                var username = user?.Username ?? "Someone";
                await _notificationService.CreateNotificationAsync(
                    rant.UserId,
                    "ReRant",
                    $"{username} reranted your rant.",
                    username,
                    rant.Id
                );
            }
        }

        return true;
    }

    public async Task<bool> ToggleBookmarkAsync(string rantId, string userId)
    {
        var rant = await _context.Rants.FirstOrDefaultAsync(r => r.PublicId == rantId);
        if (rant == null) return false;

        var existingBookmark = await _context.RantBookmarks
            .FirstOrDefaultAsync(b => b.RantId == rant.Id && b.UserId == userId);

        if (existingBookmark != null)
        {
            _context.RantBookmarks.Remove(existingBookmark);
        }
        else
        {
            _context.RantBookmarks.Add(new RantBookmark { RantId = rant.Id, UserId = userId });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<string>> GetLikesAsync(string rantId)
    {
        var rant = await _context.Rants.FirstOrDefaultAsync(r => r.PublicId == rantId);
        if (rant == null) return Enumerable.Empty<string>();

        return await _context.RantLikes
            .Where(l => l.RantId == rant.Id)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => l.UserId)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetReRantsAsync(string rantId)
    {
        var rant = await _context.Rants.FirstOrDefaultAsync(r => r.PublicId == rantId);
        if (rant == null) return Enumerable.Empty<string>();

        return await _context.RantReRants
            .Where(r => r.RantId == rant.Id)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => r.UserId)
            .ToListAsync();
    }
}
