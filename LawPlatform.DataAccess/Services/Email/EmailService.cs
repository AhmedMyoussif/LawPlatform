using Microsoft.Extensions.Logging;
using FluentEmail.Core;
using LawPlatform.Entities.Models.Auth.Users;
using CloudinaryDotNet;
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
                    LawyerEmailType.Rejected => "LawyerRejectionEmail.html",
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
                    LawyerEmailType.Rejected => "Your Lawyer Account Application Update",
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


        public async Task SendClientRegstrationEmailAsync(User client)
        {
            try
            {
                var rootPath = Directory.GetCurrentDirectory();
                var templatePath = Path.Combine(rootPath, "wwwroot", "EmailTemplates", "ClientRegstrationEmail.html");

                if (!File.Exists(templatePath))
                {
                    _logger.LogError($"Client Regestration Email Template not found at path: {templatePath}");
                    throw new FileNotFoundException("Client Regestration Email Template not found.", templatePath);
                }

                var emailTemplate = await File.ReadAllTextAsync(templatePath);

                emailTemplate = emailTemplate
                    .Replace("{Username}", client.UserName)
                    .Replace("{CurrentYear}", DateTime.UtcNow.Year.ToString());

                var sendResult = await _fluentEmail
                    .To(client.Email)
                    .Subject("Your Account Has Been Created")
                    .Body(emailTemplate, isHtml: true)
                    .SendAsync();

                if (!sendResult.Successful)
                {
                    _logger.LogError($"Failed to send Client Regestration email to {client.Email}. Errors: {string.Join(", ", sendResult.ErrorMessages)}");
                    throw new Exception("Failed to send client regestration email.");
                }

                _logger.LogInformation($"Client regestration email successfully sent to {client.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while sending client regestration email to {client.Email}");
                throw;
            }
        }
    }
}
