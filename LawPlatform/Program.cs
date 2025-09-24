
using System.Text.Json.Serialization;
using LawPlatform.DataAccess.ApplicationContext;
using LawPlatform.DataAccess.Seeder;
using LawPlatform.Entities.Models.Auth.Identity;
using LawPlatform.Entities.Shared.Bases;
using LawPlatform.Utilities.Configurations;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

using StackExchange.Redis;
using Ecommerce.API.Extensions;
using LawPlatform.DataAccess.Extensions;
using Microsoft.AspNetCore.SignalR;
using LawPlatform.API.Hubs;

namespace EcommercePlatform
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressModelStateInvalidFilter = true;
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            builder.Host.UseSerilogLogging();

            // Active Model State
            builder.Services.AddControllers().ConfigureApiBehaviorOptions(
                options => options.SuppressModelStateInvalidFilter = true
            );

            // IOptional Pattern
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JWT"));
            builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.Configure<GoogleAuthSettings>(builder.Configuration.GetSection("Authorization:Google"));

            builder.Services.AddApplicationServices(builder.Configuration);
            builder.Services.AddScoped<ResponseHandler>();
            builder.Services.AddDatabase(builder.Configuration);
            builder.Services.AddAuthenticationAndAuthorization(builder.Configuration);
            builder.Services.AddEmailServices(builder.Configuration);
          


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:3000")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

           
            builder.Services.AddFluentValidation();

            // Rate limiter for otp resend
            builder.Services.AddResendOtpRateLimiter();

            builder.Services.AddDataProtection()
                .PersistKeysToDbContext<LawPlatformContext>()
                .SetApplicationName("AuthStarter");

            // For redis 
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configuration = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis"));
                configuration.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(configuration);
            });

            builder.Services.AddSwagger();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSignalR();
            builder.Services.AddSingleton<IUserIdProvider, NameIdentifierUserIdProvider>();


            var app = builder.Build();

            #region Seed User,Role Data
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var userManager = services.GetRequiredService<UserManager<User>>();
                var roleManager = services.GetRequiredService<RoleManager<LawPlatform.Entities.Models.Auth.Identity.Role>>();

                await RoleSeeder.SeedAsync(roleManager);
                await UserSeeder.SeedAsync(userManager);
            }
            #endregion

            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            //app.UseCors(c=>c.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.UseStaticFiles(); 

            app.MapHub<LawPlatform.API.Hubs.ChatHub>("/hubs/chat");
            app.MapHub<NotificationHub>("/notificationHub");

            app.Run();
        }
    }
}
