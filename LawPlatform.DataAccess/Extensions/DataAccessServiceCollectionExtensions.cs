using System;
using System.Net.Mail;
using System.Net;
using LawPlatform.DataAccess.Services.OAuth;
using LawPlatform.DataAccess.ApplicationContext;
using LawPlatform.DataAccess.Services.Auth;
using LawPlatform.DataAccess.Services.Email;
using LawPlatform.DataAccess.Services.ImageUploading;
using LawPlatform.DataAccess.Services.Token;
using LawPlatform.Utilities.Configurations;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentEmail.Smtp;
using LawPlatform.DataAccess.Services.Admin;
using LawPlatform.DataAccess.Services.Consultation;
using LawPlatform.DataAccess.Services.OAuth;
using CloudinaryDotNet;
using LawPlatform.DataAccess.Services.Profile;
using LawPlatform.DataAccess.Services.Proposal;
using LawPlatform.DataAccess.Services.Review;
using LawPlatform.DataAccess.Services.Chat;
using LawPlatform.DataAccess.Services.Notification;
using LawPlatform.DataAccess.Services.Report;
using LawPlatform.DataAccess.Services.Payment;

namespace LawPlatform.DataAccess.Extensions
{
    public static class DataAccessServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {

            var conMode = configuration["ConnectionMode"] ?? "ProdCS";
            var conString = configuration.GetConnectionString(conMode) ??
                            throw new InvalidOperationException($"Connection string '{conMode}' not found.");
            
            services.AddDbContext<LawPlatformContext>(options =>
                options.UseSqlServer(conString));

            return services;
        }
        public static IServiceCollection AddApplicationServices(this IServiceCollection services , IConfiguration configuration)
        {
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAuthGoogleService, AuthGoogleService>();
            services.AddScoped<IFileUploadService, UploadcareService>();
            services.AddScoped<ITokenStoreService, TokenStoreService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IConsultationService, ConsultationService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IProposalService, ProposalService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IReportService, ReportService>();
            
            // Register Tamara Payment Service
            services.AddScoped<ITamaraPaymentService, TamaraPaymentService>();
            
            services.AddScoped<HttpClient>();

            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            return services;

        }

        //public static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration configuration)
        //{
        //    var emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>();

        //    services.AddFluentEmail(emailSettings.FromEmail)
        //        .AddSmtpSender(new SmtpClient(emailSettings.SmtpServer)
        //        {
        //            Port = emailSettings.SmtpPort,
        //            Credentials = new NetworkCredential(emailSettings.Username, emailSettings.Password),
        //            EnableSsl = emailSettings.EnableSsl
        //        });
        //    return services;
        //}
        public static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration configuration)
        {
            var emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>();

            services.AddFluentEmail(emailSettings.FromEmail)
                .AddSmtpSender(() => new SmtpClient(emailSettings.SmtpServer, emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(emailSettings.Username, emailSettings.Password),
                    EnableSsl = emailSettings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                });

            return services;
        }

    }
}
