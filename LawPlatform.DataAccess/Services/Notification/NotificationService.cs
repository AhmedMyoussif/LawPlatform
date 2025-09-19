using LawPlatform.DataAccess.ApplicationContext;
using Microsoft.AspNetCore.SignalR;

namespace LawPlatform.DataAccess.Services.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly LawPlatformContext _context;

        public NotificationService(LawPlatformContext context)
        {
            _context = context;
        }

        public async Task NotifyUserAsync(string userId, string title, string message)
        {
            var notification = new LawPlatform.Entities.Models.Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }
}
