using Microsoft.Extensions.Logging;
using FluentEmail.Core;
using System.IO;
using System.Threading.Tasks;
using LawPlatform.Entities.Models.Auth.Identity;
using LawPlatform.Utilities.Enums;
using LawPlatform.Entities.Models.Auth.Users;

namespace LawPlatform.DataAccess.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly IFluentEmail _fluentEmail;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IFluentEmail fluentEmail, ILogger<EmailService> logger)
        {
            _fluentEmail = fluentEmail;
            _logger = logger;
        }

        public async Task SendLawyerEmailAsync(Lawyer lawyer, LawyerEmailType emailType)
        {
            var user = lawyer.User;

            try
            {
                var rootPath = Directory.GetCurrentDirectory();
                string templateFile = emailType switch
                {
                    LawyerEmailType.Pending => "LawyerRegisterEmail.html",
                    LawyerEmailType.Approved => "LawyerApprovalEmail.html",
                    _ => throw new ArgumentException("Invalid email type")
                };

                var templatePath = Path.Combine(rootPath, "wwwroot", "EmailTemplates", templateFile);


                if (!File.Exists(templatePath))
                {
                    _logger.LogError($"Email template not found at path: {templatePath}");
                    throw new FileNotFoundException("Email template not found.", templatePath);
                }

                var emailTemplate = await File.ReadAllTextAsync(templatePath);

                emailTemplate = emailTemplate
                    .Replace("{Username}", user.UserName ?? user.Email ?? "Lawyer")
                    .Replace("{CurrentYear}", DateTime.UtcNow.Year.ToString());

                string subject = emailType switch
                {
                    LawyerEmailType.Pending => "Your Lawyer Account Is Pending Approval",
                    LawyerEmailType.Approved => "Your Lawyer Account Has Been Approved",
                    _ => "Lawyer Account Update"
                };

                var sendResult = await _fluentEmail
                    .To(user.Email)
                    .Subject(subject)
                    .Body(emailTemplate, isHtml: true)
                    .SendAsync();

                if (!sendResult.Successful)
                {
                    _logger.LogError($"Failed to send {emailType} email to {user.Email}. Errors: {string.Join(", ", sendResult.ErrorMessages)}");
                    throw new Exception($"Failed to send {emailType} email.");
                }

                _logger.LogInformation($"{emailType} email successfully sent to {user.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while sending {emailType} email to {user.Email}");
                throw;
            }
        }
    }
}
