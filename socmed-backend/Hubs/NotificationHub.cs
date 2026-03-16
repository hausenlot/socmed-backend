using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace socmed_backend.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    // Hub is mainly for client-to-server or server-to-client messaging.
    // For now, we only need it to allow users to join their own private 'group'
    // so we can send targeted notifications.
    
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
        await base.OnConnectedAsync();
    }
}
