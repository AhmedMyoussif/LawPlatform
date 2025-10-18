using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPlatform.DataAccess.Services.Notification
{
    public interface INotificationService
    {
        Task NotifyUserAsync(string userId, string title, string message);
    }

}
