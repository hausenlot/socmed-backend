using socmed_backend.DTOs;

namespace socmed_backend.Services;

public interface INotificationService
{
    Task<IEnumerable<NotificationResponseDto>> GetNotificationsAsync(string userId, int page = 1, int pageSize = 20);
    Task<int> GetUnreadCountAsync(string userId);
    Task<bool> MarkAsReadAsync(int notificationId, string userId);
    Task<bool> MarkAllAsReadAsync(string userId);
    Task CreateNotificationAsync(string userId, string type, string message, string? sourceUsername = null, int? relatedEntityId = null);
}
