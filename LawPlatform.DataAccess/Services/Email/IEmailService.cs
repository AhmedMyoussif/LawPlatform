using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Entities.Models.Auth.Identity;

namespace LawPlatform.DataAccess.Services.Email
{
    public interface IEmailService
    {
         Task SendLawyerApprovalEmailAsync(User lawyer);
        Task SendClientRegstrationEmailAsync(User client);
    }
}
