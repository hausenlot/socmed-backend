using Microsoft.EntityFrameworkCore;
using socmed_backend.Data;
using socmed_backend.DTOs;
using socmed_backend.Models;

namespace socmed_backend.Services;

public class FollowService : IFollowService
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;

    public FollowService(AppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<bool> ToggleFollowAsync(string followerId, string followingUsername)
    {
        var followingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == followingUsername.ToLower());

        if (followingUser == null || followingUser.Id == followerId) return false;

        var existingFollow = await _context.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingUser.Id);

        if (existingFollow != null)
        {
            // Unfollow
            _context.Follows.Remove(existingFollow);
            await _context.SaveChangesAsync();
            return true;
        }

        // Follow
        var follow = new Follow
        {
            FollowerId = followerId,
            FollowingId = followingUser.Id
        };

        _context.Follows.Add(follow);
        await _context.SaveChangesAsync();

        // Notification
        var follower = await _context.Users.FindAsync(followerId);
        var followerUsername = follower?.Username ?? "Someone";
        
        await _notificationService.CreateNotificationAsync(
            followingUser.Id,
            "Follow",
            $"{followerUsername} followed you.",
            followerUsername
        );

        return true;
    }

    public async Task<IEnumerable<UserProfileDto>> GetFollowersAsync(string username, int page = 1, int pageSize = 20, string? requestingUserId = null)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

        if (user == null) return new List<UserProfileDto>();

        return await _context.Follows
            .Include(f => f.Follower)
            .Where(f => f.FollowingId == user.Id)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new UserProfileDto
            {
                Username = f.Follower.Username,
                DisplayName = f.Follower.DisplayName,
                Bio = f.Follower.Bio,
                ProfileImageUrl = f.Follower.ProfileImageUrl,
                CreatedAt = f.Follower.CreatedAt,
                IsFollowedByMe = requestingUserId != null && _context.Follows.Any(f2 => f2.FollowerId == requestingUserId && f2.FollowingId == f.FollowerId)
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<UserProfileDto>> GetFollowingAsync(string username, int page = 1, int pageSize = 20, string? requestingUserId = null)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

        if (user == null) return new List<UserProfileDto>();

        return await _context.Follows
            .Include(f => f.Following)
            .Where(f => f.FollowerId == user.Id)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new UserProfileDto
            {
                Username = f.Following.Username,
                DisplayName = f.Following.DisplayName,
                Bio = f.Following.Bio,
                ProfileImageUrl = f.Following.ProfileImageUrl,
                CreatedAt = f.Following.CreatedAt,
                IsFollowedByMe = requestingUserId != null && _context.Follows.Any(f2 => f2.FollowerId == requestingUserId && f2.FollowingId == f.FollowingId)
            })
            .ToListAsync();
    }
}
