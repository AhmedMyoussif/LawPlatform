//using System;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using LawPlatform.DataAccess.ApplicationContext;
//using LawPlatform.DataAccess.Services.ImageUploading;
//using LawPlatform.Entities.DTO.Profile;
//using LawPlatform.Entities.Models;
//using LawPlatform.Entities.Models.Auth.Users;
//using LawPlatform.Entities.Shared.Bases;
//using Microsoft.AspNetCore.Http;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;

//namespace LawPlatform.DataAccess.Services.Profile
//{
//    public class ProfileService : IProfileService
//    {
//        private readonly ILogger<ProfileService> _logger;
//        private readonly LawPlatformContext _context;
//        private readonly ResponseHandler _responseHandler;
//        private readonly IImageUploadService _imageUploadService;
//        private readonly IHttpContextAccessor _httpContextAccessor;

//        public ProfileService(
//            ILogger<ProfileService> logger,
//            LawPlatformContext context,
//            ResponseHandler responseHandler,
//            IImageUploadService imageUploadService,
//            IHttpContextAccessor httpContextAccessor)
//        {
//            _logger = logger;
//            _context = context;
//            _responseHandler = responseHandler;
//            _imageUploadService = imageUploadService;
//            _httpContextAccessor = httpContextAccessor;
//        }

//        #region Helpers

      
//        private (string? UserIdClaim, string? EntityIdClaim) GetClaimsIdentifiers()
//        {
//            var user = _httpContextAccessor.HttpContext?.User;
//            if (user == null) return (null, null);

//            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
//                         ?? user.FindFirst("nameid")?.Value;

//            var entityId = user.FindFirst("EntityId")?.Value
//                           ?? user.FindFirst("entityid")?.Value;

//            return (userId, entityId);
//        }

    
//        private async Task<string?> ResolveAspNetUserIdAsync(string? userIdClaim, string? entityIdClaim)
//        {
//            // If the claim likely contains the actual AspNetUsers.Id, try check existence quickly:
//            if (!string.IsNullOrEmpty(userIdClaim))
//            {
//                var exists = await _context.Users.AnyAsync(u => u.Id == userIdClaim);
//                if (exists) return userIdClaim;
//            }

//            // If we have entity id, try to resolve to the related user
//            if (!string.IsNullOrEmpty(entityIdClaim))
//            {
//                var client = await _context.Clients.AsNoTracking()
//                    .FirstOrDefaultAsync(c => c.Id == entityIdClaim);
//                if (client != null) return client.UserId;

//                var lawyer = await _context.Lawyers.AsNoTracking()
//                    .FirstOrDefaultAsync(l => l.Id == entityIdClaim);
//                if (lawyer != null) return lawyer.UserId;
//            }

//            // fallback: if userIdClaim exists but not found in AspNetUsers, return null
//            return null;
//        }

//        #endregion

//        public async Task<Response<object>> GetProfileAsync()
//        {
//            var (userIdClaim, entityIdClaim) = GetClaimsIdentifiers();

//            if (string.IsNullOrEmpty(userIdClaim) && string.IsNullOrEmpty(entityIdClaim))
//            {
//                _logger.LogWarning("Unauthorized access attempt to GetProfile (no claims).");
//                return _responseHandler.Unauthorized<object>("You are not authorized to view this profile.");
//            }

//            var resolvedUserId = await ResolveAspNetUserIdAsync(userIdClaim, entityIdClaim);
//            if (string.IsNullOrEmpty(resolvedUserId))
//            {
//                _logger.LogWarning("Could not resolve AspNet UserId from claims. NameId: {NameId}, EntityId: {EntityId}", userIdClaim, entityIdClaim);
//                return _responseHandler.Unauthorized<object>("You are not authorized to view this profile.");
//            }

//            // Try client first
//            var client = await _context.Clients
//                .Include(c => c.User)
//                .Include(c => c.ProfileImage)
//                .FirstOrDefaultAsync(c => c.UserId == resolvedUserId);

//            if (client != null)
//            {
//                var clientresponse = new ClientProfileResponse
//                {
//                    FirstName = client.FirstName,
//                    LastName = client.LastName,
//                    Address = client.Address,
//                    Email = client.User?.Email,
//                    Role = "Client",
//                    ProfileImageUrl = client.ProfileImage?.ImageUrl,
//                };

//                return _responseHandler.Success<object>(clientresponse, "Profile fetched successfully");
//            }

//            // Then try lawyer
//            var lawyer = await _context.Lawyers
//                .Include(l => l.User)
//                .Include(l => l.ProfileImage)
//                .FirstOrDefaultAsync(l => l.UserId == resolvedUserId);

//            if (lawyer != null)
//            {
//                var lawyerresponse = new LawyerProfileResponse
//                {
//                    Id = lawyer.Id,
//                    FullName = lawyer.FirstName + " " + lawyer.LastName,
//                    Role = "Lawyer",
//                    UserName = lawyer.User?.UserName,
//                    Age = lawyer.Age,
//                    Address = lawyer.Address,
//                    Email = lawyer.User?.Email,
//                    Specialization = lawyer.Specialization,
//                    ProfileImageUrl = lawyer.ProfileImage?.ImageUrl,
//                };

//                return _responseHandler.Success<object>(lawyerresponse, "Profile fetched successfully");
//            }

//            _logger.LogWarning("User with resolved UserId {UserId} not found as Client or Lawyer", resolvedUserId);
//            return _responseHandler.NotFound<object>("User not found as Client or Lawyer");
//        }

//        public async Task<Response<bool>> UpdateProfileAsync(string userId, UpdateClientProfileRequest dto)
//        {
//            // Ensure the caller is authorized to update this profile:
//            var (userIdClaim, entityIdClaim) = GetClaimsIdentifiers();
//            var resolvedCallerUserId = await ResolveAspNetUserIdAsync(userIdClaim, entityIdClaim);

//            if (string.IsNullOrEmpty(resolvedCallerUserId))
//            {
//                _logger.LogWarning("Unauthorized attempt to update profile (could not resolve caller).");
//                return _responseHandler.Unauthorized<bool>("You are not authorized to update this profile.");
//            }

//            // caller can update only their own profile (or extend here to allow admins)
//            if (!string.Equals(resolvedCallerUserId, userId, StringComparison.OrdinalIgnoreCase))
//            {
//                _logger.LogWarning("User {Caller} attempted to update profile of {Target}", resolvedCallerUserId, userId);
//                return _responseHandler.Unauthorized<bool>("You are not authorized to update this profile.");
//            }

//            var client = await _context.Clients
//                .Include(c => c.User)
//                .FirstOrDefaultAsync(c => c.UserId == userId);

//            if (client == null)
//            {
//                _logger.LogWarning("Client with UserId {UserId} not found", userId);
//                return _responseHandler.BadRequest<bool>("Client not found");
//            }

//            try
//            {
//                client.FirstName = dto.FirstName;
//                client.LastName = dto.LastName;
//                client.Address = dto.Address;

//                // EF is tracking client; calling Update is optional but harmless
//                _context.Clients.Update(client);
//                await _context.SaveChangesAsync();

//                return _responseHandler.Success(true, "Profile updated successfully");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error updating profile for UserId {UserId}", userId);
//                return _responseHandler.BadRequest<bool>("An error occurred while updating the profile");
//            }
//        }

//        // (Optional) Add UpdateProfileImageAsync here similar to your commented method, using the same claims resolution.
//    }
//}
