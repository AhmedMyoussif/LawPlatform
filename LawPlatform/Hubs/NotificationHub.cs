using LawPlatform.Entities.Models.Auth.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace LawPlatform.API.Hubs;

[Authorize]
// NotificationHub.cs
[Authorize]
public class NotificationHub : Hub
{
    public async Task SendNotification(string userId, string title, string message)
    {
        await Clients.User(userId).SendAsync("ReceiveNotification", title, message);
    }
}

