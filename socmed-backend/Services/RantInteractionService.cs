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

    public async Task<bool> ToggleLikeAsync(int rantId, string userId)
    {
        var rantExists = await _context.Rants.AnyAsync(r => r.Id == rantId);
        if (!rantExists) return false;

        var existingLike = await _context.RantLikes
            .FirstOrDefaultAsync(l => l.RantId == rantId && l.UserId == userId);

        if (existingLike != null)
        {
            _context.RantLikes.Remove(existingLike);
            await _context.SaveChangesAsync();
        }
        else
        {
            _context.RantLikes.Add(new RantLike { RantId = rantId, UserId = userId });
            await _context.SaveChangesAsync();

            // Trigger notification
            var rant = await _context.Rants.FindAsync(rantId);
            if (rant != null && rant.UserId != userId)
            {
                var user = await _context.Users.FindAsync(userId);
                var username = user?.Username ?? "Someone";
                await _notificationService.CreateNotificationAsync(
                    rant.UserId,
                    "Like",
                    $"{username} liked your rant.",
                    username,
                    rantId
                );
            }
        }

        return true;
    }

    public async Task<bool> ToggleReRantAsync(int rantId, string userId)
    {
        var rantExists = await _context.Rants.AnyAsync(r => r.Id == rantId);
        if (!rantExists) return false;

        var existingReRant = await _context.RantReRants
            .FirstOrDefaultAsync(r => r.RantId == rantId && r.UserId == userId);

        if (existingReRant != null)
        {
            _context.RantReRants.Remove(existingReRant);
            await _context.SaveChangesAsync();
        }
        else
        {
            _context.RantReRants.Add(new RantReRant { RantId = rantId, UserId = userId });
            await _context.SaveChangesAsync();

            // Trigger notification
            var rant = await _context.Rants.FindAsync(rantId);
            if (rant != null && rant.UserId != userId)
            {
                var user = await _context.Users.FindAsync(userId);
                var username = user?.Username ?? "Someone";
                await _notificationService.CreateNotificationAsync(
                    rant.UserId,
                    "ReRant",
                    $"{username} reranted your rant.",
                    username,
                    rantId
                );
            }
        }

        return true;
    }

    public async Task<bool> ToggleBookmarkAsync(int rantId, string userId)
    {
        var rantExists = await _context.Rants.AnyAsync(r => r.Id == rantId);
        if (!rantExists) return false;

        var existingBookmark = await _context.RantBookmarks
            .FirstOrDefaultAsync(b => b.RantId == rantId && b.UserId == userId);

        if (existingBookmark != null)
        {
            _context.RantBookmarks.Remove(existingBookmark);
        }
        else
        {
            _context.RantBookmarks.Add(new RantBookmark { RantId = rantId, UserId = userId });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<string>> GetLikesAsync(int rantId)
    {
        return await _context.RantLikes
            .Where(l => l.RantId == rantId)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => l.UserId)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetReRantsAsync(int rantId)
    {
        return await _context.RantReRants
            .Where(r => r.RantId == rantId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => r.UserId)
            .ToListAsync();
    }
}
