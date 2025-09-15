using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.DataAccess.ApplicationContext;
using LawPlatform.DataAccess.Services.ImageUploading;
using LawPlatform.Entities.DTO.Profile;
using LawPlatform.Entities.Models;
using LawPlatform.Entities.Models.Auth.Users;
using LawPlatform.Entities.Shared.Bases;
using LawPlatform.Utilities.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LawPlatform.DataAccess.Services.Profile
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileService _clientProfileService;
        private readonly ILogger<ProfileService> _logger;
        private readonly LawPlatformContext _context;
        private readonly ResponseHandler _responseHandler;
        private readonly IImageUploadService _imageUploadService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ProfileService(ILogger<ProfileService> logger, LawPlatformContext context, ResponseHandler responseHandler, IImageUploadService imageUploadService, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _context = context;
            _responseHandler = responseHandler;
            _imageUploadService = imageUploadService;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<Response<object>> GetProfileAsync()
        {
            var userIdFromToken = _httpContextAccessor.HttpContext.User
                .Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;


            if (userIdFromToken == null)
            {
                _logger.LogWarning("Unauthorized access attempt for UserId {UserId}", userIdFromToken);
                return _responseHandler.Unauthorized<object>("You are not authorized to view this profile.");
            }

            var client = await _context.Clients
                .Include(c => c.User)
                .Include(c => c.ProfileImage)
                .FirstOrDefaultAsync(c => c.User.Id == userIdFromToken);

            if (client != null)
            {
                var clientresponse = new ClientProfileResponse
                {
                    
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    Address = client.Address,
                    Email = client.User?.Email,
                    Role = "Client",
                    ProfileImageUrl = client.ProfileImage?.ImageUrl,
                };

                return _responseHandler.Success<object>(clientresponse, "Profile fetched successfully");
            }

            var lawyer = await _context.Lawyers
                .Include(l => l.User)
                .Include(l => l.ProfileImage)
                .FirstOrDefaultAsync(l => l.User.Id == userIdFromToken);

            if (lawyer != null)
            {
                var lawyerresponse = new LawyerProfileResponse
                {
                    Id = lawyer.Id,
                    FullName = lawyer.FirstName + " " + lawyer.LastName,
                    Role = "Lawyer",
                    UserName = lawyer.User.UserName,
                    Age = lawyer.Age,
                    Address = lawyer.Address,
                    Email = lawyer.User?.Email,
                    Specialization = lawyer.Specialization,
                    ProfileImageUrl = lawyer.ProfileImage?.ImageUrl,
                };

                return _responseHandler.Success<object>(lawyerresponse, "Profile fetched successfully");
            }

            _logger.LogWarning("User with UserId {UserId} not found as Client or Lawyer", userIdFromToken);
            return _responseHandler.NotFound<object>("User not found as Client or Lawyer");
        }

        public async Task<Response<bool>> UpdateProfileAsync(string userId, UpdateClientProfileRequest dto)
        {
            var client = await _context.Clients
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.User.Id == userId);

            if (client == null)
            {
                _logger.LogWarning("Client with UserId {UserId} not found", userId);
                return _responseHandler.BadRequest<bool>("Client not found");
            }

            try
            {
               
                client.FirstName = dto.FirstName;
                client.LastName = dto.LastName;
                client.Address = dto.Address;

              
                _context.Clients.Update(client);
                await _context.SaveChangesAsync();

                return _responseHandler.Success(true, "Profile updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for UserId {UserId}", userId);
                return _responseHandler.BadRequest<bool>("An error occurred while updating the profile");
            }
        }


        //public async Task<Response<string>> UpdateProfileImageAsync(string userId, IFormFile newImage)
        //{
        //    if (newImage == null || newImage.Length == 0)
        //        return _responseHandler.BadRequest<string>("No image file provided.");

        //    var client = await _context.Clients
        //        .Include(c => c.ProfileImage)
        //        .FirstOrDefaultAsync(c => c.User.Id == userId);

        //    if (client == null)
        //        return _responseHandler.BadRequest<string>("Client not found");

        //    try
        //    {
        //        string oldPublicId = null;
        //        if (client.ProfileImage != null && client.ProfileImage.ImageUrl != null)
        //        {
        //            oldPublicId = client.ProfileImage.ImageUrlPublicId; 
        //        }

        //        // رفع الصورة الجديدة
        //        var uploadResult = await _imageUploadService.UploadAsync(newImage);

        //        // حذف الصورة القديمة باستخدام PublicId مباشرة
        //        if (!string.IsNullOrEmpty(oldPublicId))
        //        {
        //            await _imageUploadService.DeleteAsync(oldPublicId);
        //        }

        //        // إنشاء أو تحديث ProfileImage
        //        if (client.ProfileImage == null)
        //            client.ProfileImage = new ProfileImage();

        //        client.ProfileImage.ImageUrl = uploadResult.Url;
        //        // لو أضفت الحقل الجديد:
        //        // client.ProfileImage.ImageUrlPublicId = uploadResult.PublicId;
        //        client.ProfileImage.UpdatedAt = DateTime.UtcNow;

        //        _context.Clients.Update(client);
        //        await _context.SaveChangesAsync();

        //        return _responseHandler.Success(client.ProfileImage.ImageUrl, "Profile image updated successfully");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error updating profile image for UserId {UserId}", userId);
        //        return _responseHandler.BadRequest<string>("An error occurred while updating the profile image");
        //    }
        //}


    }
}
