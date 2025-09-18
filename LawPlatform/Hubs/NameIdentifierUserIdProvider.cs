using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace LawPlatform.API.Hubs
{
    public class NameIdentifierUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var id = connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? connection.User?.FindFirst("nemeid")?.Value;
            return id;
        }
    }
}
