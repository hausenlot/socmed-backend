using Microsoft.EntityFrameworkCore;
using socmed_backend.Data;
using socmed_backend.DTOs;
using socmed_backend.Models;

namespace socmed_backend.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly RantService _rantService;

    public UserService(AppDbContext context, RantService rantService)
    {
        _context = context;
        _rantService = rantService;
    }

    public async Task<UserProfileDto?> GetUserByUsernameAsync(string username, string? requestingUserId = null)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

        if (user == null) return null;

        var followerCount = await _context.Follows.CountAsync(f => f.FollowingId == user.Id);
        var followingCount = await _context.Follows.CountAsync(f => f.FollowerId == user.Id);
        var rantCount = await _context.Rants.CountAsync(r => r.UserId == user.Id);

        bool isFollowedByMe = false;
        if (requestingUserId != null && requestingUserId != user.Id)
        {
            isFollowedByMe = await _context.Follows
                .AnyAsync(f => f.FollowerId == requestingUserId && f.FollowingId == user.Id);
        }

        return new UserProfileDto
        {
            Username = user.Username,
            DisplayName = user.DisplayName,
            Bio = user.Bio,
            ProfileImageUrl = user.ProfileImageUrl,
            CreatedAt = user.CreatedAt,
            FollowerCount = followerCount,
            FollowingCount = followingCount,
            RantCount = rantCount,
            IsFollowedByMe = isFollowedByMe
        };
    }

    public async Task<IEnumerable<RantResponseDto>> GetUserRantsAsync(
        string username, int page = 1, int pageSize = 10, string? requestingUserId = null)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

        if (user == null) return new List<RantResponseDto>();

        // User's own rants
        var ownRants = await _context.Rants
            .Include(r => r.User)
            .Where(r => r.UserId == user.Id)
            .Select(r => new { Rant = r, ReRantedBy = (string?)null })
            .ToListAsync();

        // Rants re-ranted by this user
        var rerantedRants = await _context.RantReRants
            .Include(rr => rr.Rant)
                .ThenInclude(r => r.User)
            .Where(rr => rr.UserId == user.Id)
            .Select(rr => new { Rant = rr.Rant, ReRantedBy = (string?)user.Username })
            .ToListAsync();

        var combinedAndSorted = ownRants.Concat(rerantedRants)
            .OrderByDescending(x => x.Rant.CreatedAt)
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

    public async Task<IEnumerable<User>> SearchUsersAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return new List<User>();

        string lowerQuery = query.ToLower();
        return await _context.Users
            .Where(u => u.Username.ToLower().Contains(lowerQuery) ||
                        (u.DisplayName != null && u.DisplayName.ToLower().Contains(lowerQuery)))
            .Take(20)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReplyResponseDto>> GetUserRepliesAsync(
        string username, int page = 1, int pageSize = 10)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

        if (user == null) return new List<ReplyResponseDto>();

        var replies = await _context.RantReplies
            .Include(r => r.User)
            .Where(r => r.UserId == user.Id && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return replies.Select(r => new ReplyResponseDto
        {
            Id = r.Id,
            Content = r.Content,
            CreatedAt = r.CreatedAt,
            UserId = r.UserId,
            Username = r.User.Username,
            DisplayName = r.User.DisplayName,
            ProfileImageUrl = r.User.ProfileImageUrl,
            LikeCount = 0,
            ReplyCount = 0,
            IsLikedByMe = false
        }).ToList();
    }

    public async Task<IEnumerable<RantResponseDto>> GetUserLikedRantsAsync(
        string username, int page = 1, int pageSize = 10, string? requestingUserId = null)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

        if (user == null) return new List<RantResponseDto>();

        var rants = await _context.RantLikes
            .Include(l => l.Rant)
                .ThenInclude(r => r.User)
            .Where(l => l.UserId == user.Id)
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => l.Rant)
            .ToListAsync();

        return await _rantService.MapToResponseDtosAsync(rants, requestingUserId);
    }

    public async Task<IEnumerable<UserProfileDto>> GetSuggestedUsersAsync(string? requestingUserId, int count = 5)
    {
        // If not logged in, just return some recent users
        IQueryable<User> query = _context.Users;

        if (requestingUserId != null)
        {
            // Exclude self and already-followed users
            var followingIds = await _context.Follows
                .Where(f => f.FollowerId == requestingUserId)
                .Select(f => f.FollowingId)
                .ToListAsync();

            followingIds.Add(requestingUserId);
            query = query.Where(u => !followingIds.Contains(u.Id));
        }

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Take(count)
            .ToListAsync();

        return users.Select(u => new UserProfileDto
        {
            Username = u.Username,
            DisplayName = u.DisplayName,
            Bio = u.Bio,
            ProfileImageUrl = u.ProfileImageUrl,
            CreatedAt = u.CreatedAt,
            FollowerCount = _context.Follows.Count(f => f.FollowingId == u.Id),
            FollowingCount = _context.Follows.Count(f => f.FollowerId == u.Id),
            RantCount = _context.Rants.Count(r => r.UserId == u.Id),
            IsFollowedByMe = false
        }).ToList();
    }

    public async Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        if (dto.DisplayName != null) user.DisplayName = dto.DisplayName;
        if (dto.Bio != null) user.Bio = dto.Bio;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateProfileImageAsync(string userId, IFormFile file, string webRootPath, string baseUrl)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        // Delete old file if exists
        if (!string.IsNullOrEmpty(user.ProfileImageUrl))
        {
            var oldRelativePath = user.ProfileImageUrl.Replace(baseUrl, "").TrimStart('/');
            var oldFilePath = Path.Combine(webRootPath, oldRelativePath);
            if (File.Exists(oldFilePath)) File.Delete(oldFilePath);
        }

        // Save new file
        var uploadsDir = Path.Combine(webRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{userId}_{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        user.ProfileImageUrl = $"{baseUrl}/uploads/{fileName}";
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveProfileImageAsync(string userId, string webRootPath)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        if (!string.IsNullOrEmpty(user.ProfileImageUrl))
        {
            // Extract filesystem path from URL
            var uri = new Uri(user.ProfileImageUrl);
            var relativePath = uri.AbsolutePath.TrimStart('/');
            var filePath = Path.Combine(webRootPath, relativePath);
            if (File.Exists(filePath)) File.Delete(filePath);
        }

        user.ProfileImageUrl = null;
        await _context.SaveChangesAsync();
        return true;
    }
}
