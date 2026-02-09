using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CarAds.Hubs
{
    public class CarAdHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // userId از Claims (تو AuthController گذاشتی)
            var userId = Context.User?.FindFirst("userId")?.Value;
            if (!string.IsNullOrWhiteSpace(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User:{userId}");
            }

            // Role از Claims
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            if (role == "Admin" || role == "SuperAdmin")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }

            await base.OnConnectedAsync();
        }
    }
}
