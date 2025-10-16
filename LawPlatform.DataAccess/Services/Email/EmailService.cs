using Microsoft.Extensions.Logging;
using FluentEmail.Core;
using LawPlatform.Entities.Models.Auth.Users;
using CloudinaryDotNet;
using LawPlatform.Entities.Models.Auth.Identity;
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

        public async Task SendLawyerApprovalEmailAsync(User lawyer)
        {
            try
            {
                var rootPath = Directory.GetCurrentDirectory();
                var templatePath = Path.Combine(rootPath, "wwwroot", "EmailTemplates", "LawyerApprovalEmail.html");

                if (!File.Exists(templatePath))
                {
                    _logger.LogError($"Lawyer Approval Email Template not found at path: {templatePath}");
                    throw new FileNotFoundException("Lawyer Approval Email Template not found.", templatePath);
                }

                var emailTemplate = await File.ReadAllTextAsync(templatePath);

                emailTemplate = emailTemplate
                    .Replace("{Username}", lawyer.UserName)
                    .Replace("{CurrentYear}", DateTime.UtcNow.Year.ToString());

                var sendResult = await _fluentEmail
                    .To(lawyer.Email)
                    .Subject("Your Lawyer Account Has Been Approved")
                    .Body(emailTemplate, isHtml: true)
                    .SendAsync();

                if (!sendResult.Successful)
                {
                    _logger.LogError($"Failed to send approval email to {lawyer.Email}. Errors: {string.Join(", ", sendResult.ErrorMessages)}");
                    throw new Exception("Failed to send lawyer approval email.");
                }

                _logger.LogInformation($"Lawyer approval email successfully sent to {lawyer.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while sending lawyer approval email to {lawyer.Email}");
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
