using Microsoft.EntityFrameworkCore;
using socmed_backend.Data;
using socmed_backend.DTOs;
using socmed_backend.Models;

using Microsoft.AspNetCore.SignalR;
using socmed_backend.Hubs;

namespace socmed_backend.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IMultimediaService _multimediaService;

    public NotificationService(AppDbContext context, IHubContext<NotificationHub> hubContext, IMultimediaService multimediaService)
    {
        _context = context;
        _hubContext = hubContext;
        _multimediaService = multimediaService;
    }

    public async Task<IEnumerable<NotificationResponseDto>> GetNotificationsAsync(string userId, int page = 1, int pageSize = 20)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var relatedRantIds = notifications
            .Where(n => n.RelatedEntityId.HasValue)
            .Select(n => n.RelatedEntityId!.Value)
            .Distinct()
            .ToList();

        var rantInfoMap = await _context.Rants
            .Where(r => relatedRantIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => new { r.PublicId, r.Content });

        var sourceUsernames = notifications
            .Where(n => !string.IsNullOrEmpty(n.SourceUsername))
            .Select(n => n.SourceUsername!)
            .Distinct()
            .ToList();

        var sourceUsersMap = await _context.Users
            .Where(u => sourceUsernames.Contains(u.Username))
            .ToDictionaryAsync(u => u.Username, u => new { u.DisplayName, u.ProfileMediaId });

        return notifications.Select(n => {
            string? publicRantId = null;
            string? relatedRantContent = null;
            if (n.RelatedEntityId.HasValue && rantInfoMap.TryGetValue(n.RelatedEntityId.Value, out var rantInfo))
            {
                publicRantId = rantInfo.PublicId;
                relatedRantContent = rantInfo.Content;
            }

            string? sourceDisplayName = null;
            string? sourceProfileImageUrl = null;
            if (!string.IsNullOrEmpty(n.SourceUsername) && sourceUsersMap.TryGetValue(n.SourceUsername, out var userVal))
            {
                sourceDisplayName = userVal.DisplayName;
                sourceProfileImageUrl = userVal.ProfileMediaId != null 
                    ? _multimediaService.GetPublicUrl(userVal.ProfileMediaId) 
                    : null;
            }

            return new NotificationResponseDto
            {
                Id = n.Id,
                Type = n.Type,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                SourceUsername = n.SourceUsername,
                SourceDisplayName = sourceDisplayName,
                SourceProfileImageUrl = sourceProfileImageUrl,
                RantId = publicRantId,
                RelatedRantContent = relatedRantContent
            };
        }).ToList();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, string userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null) return false;

        notification.IsRead = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(string userId)
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        if (unreadNotifications.Count == 0) return true;

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task CreateNotificationAsync(string userId, string type, string message, string? sourceUsername = null, int? relatedEntityId = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Message = message,
            SourceUsername = sourceUsername,
            RelatedEntityId = relatedEntityId
        };
 
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // Push to SignalR
        string? publicRantId = null;
        string? relatedRantContent = null;
        if (notification.RelatedEntityId.HasValue)
        {
            var rant = await _context.Rants.FindAsync(notification.RelatedEntityId.Value);
            publicRantId = rant?.PublicId;
            relatedRantContent = rant?.Content;
        }

        string? sourceDisplayName = null;
        string? sourceProfileImageUrl = null;
        if (!string.IsNullOrEmpty(sourceUsername))
        {
            var sourceUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == sourceUsername);
            if (sourceUser != null)
            {
                sourceDisplayName = sourceUser.DisplayName;
                sourceProfileImageUrl = sourceUser.ProfileMediaId != null 
                    ? _multimediaService.GetPublicUrl(sourceUser.ProfileMediaId) 
                    : null;
            }
        }

        var dto = new NotificationResponseDto
        {
            Id = notification.Id,
            Type = notification.Type,
            Message = notification.Message,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            SourceUsername = notification.SourceUsername,
            SourceDisplayName = sourceDisplayName,
            SourceProfileImageUrl = sourceProfileImageUrl,
            RantId = publicRantId,
            RelatedRantContent = relatedRantContent
        };

        await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", dto);
    }
 
    public async Task ProcessMentionsAsync(string content, string sourceUserId, int? relatedEntityId, IEnumerable<string>? excludeUserIds = null)
    {
        if (string.IsNullOrWhiteSpace(content)) return;
 
        var mentionRegex = new System.Text.RegularExpressions.Regex(@"@(\w+)");
        var matches = mentionRegex.Matches(content);
        if (matches.Count == 0) return;
 
        var mentionedUsernames = matches.Cast<System.Text.RegularExpressions.Match>()
            .Select(m => m.Groups[1].Value.ToLower())
            .Distinct()
            .ToList();
 
        var sourceUser = await _context.Users.FindAsync(sourceUserId);
        var sourceUsername = sourceUser?.Username ?? "Someone";
 
        foreach (var username in mentionedUsernames)
        {
            // Don't notify yourself
            if (sourceUser != null && username == sourceUser.Username.ToLower()) continue;
 
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username);
            if (user != null)
            {
                // Skip if user is in exclusion list (e.g. they already got a Reply notification)
                if (excludeUserIds != null && excludeUserIds.Contains(user.Id)) continue;

                await CreateNotificationAsync(
                    user.Id,
                    "Mention",
                    $"{sourceUsername} mentioned you in a rant.",
                    sourceUsername,
                    relatedEntityId
                );
            }
        }
    }
}
