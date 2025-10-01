using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Entities.Models.Auth.Identity;
using LawPlatform.Entities.Models.Auth.Users;
using LawPlatform.Utilities.Enums;

namespace LawPlatform.DataAccess.Services.Email
{
    public interface IEmailService
    {
        Task SendLawyerEmailAsync(Lawyer lawyer, LawyerEmailType emailType);
    }
}
