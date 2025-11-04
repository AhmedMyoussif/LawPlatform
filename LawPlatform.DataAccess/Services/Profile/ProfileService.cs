using LawPlatform.DataAccess.ApplicationContext;
using LawPlatform.DataAccess.Services.ImageUploading;
using LawPlatform.Entities.DTO.Profile;
using LawPlatform.Entities.Models;
using LawPlatform.Entities.Shared.Bases;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace LawPlatform.DataAccess.Services.Profile
{
    public class ProfileService : IProfileService
    {
        private readonly ILogger<ProfileService> _logger;
        private readonly LawPlatformContext _context;
        private readonly ResponseHandler _responseHandler;
        private readonly IFileUploadService _imageUploadService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProfileService(
            ILogger<ProfileService> logger,
            LawPlatformContext context,
            ResponseHandler responseHandler,
            IFileUploadService imageUploadService,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _context = context;
            _responseHandler = responseHandler;
            _imageUploadService = imageUploadService;
            _httpContextAccessor = httpContextAccessor;
        }

        #region Helpers


        private string? GetUserIdClaim()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return null;

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? user.FindFirst("nameid")?.Value;

            return userId;
        }
        private async Task<bool> UserExistsAsync(string userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }

        #endregion

        public async Task<Response<object>> GetProfileAsync()
        {
            var userId = GetUserIdClaim();
            var role = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userId))
                return _responseHandler.Unauthorized<object>("You are not authorized to view this profile.");

            var userExists = await UserExistsAsync(userId);
            if (!userExists)
                return _responseHandler.Unauthorized<object>("You are not authorized to view this profile.");

            if (role == "Client")
            {
                var client = await _context.Clients
                    .Where(c => !c.IsDeleted)
                    .Include(c => c.User)
                    .Include(c => c.ProfileImage)
                    .FirstOrDefaultAsync(c => c.Id == userId);

                if (client != null)
                {
                    var clientResponse = new ClientProfileResponse
                    {
                        FirstName = client.FirstName,
                        LastName = client.LastName,
                        Address = client.Address,
                        Email = client.User?.Email,
                        Role = "Client",
                        ProfileImageUrl = client.ProfileImage?.ImageUrl,
                    };

                    return _responseHandler.Success<object>(clientResponse, "Profile fetched successfully");
                }

            }
            else if ((role == "Lawyer"))
            {
                var lawyer = await _context.Lawyers
                    .Where(l => !l.IsDeleted)
                    .Include(l => l.User)
                    .Include(l => l.ProfileImage)
                    .FirstOrDefaultAsync(l => l.Id == userId);

                if (lawyer != null)
                {
                    var lawyerResponse = new LawyerProfileResponse
                    {
                        Id = lawyer.Id,
                        FullName = lawyer.FirstName + " " + lawyer.LastName,
                        Role = "Lawyer",
                        UserName = lawyer.User?.UserName,
                        Age = lawyer.Age,
                        Address = lawyer.Address,
                        Email = lawyer.User?.Email,
                        PhoneNumber = lawyer.User?.PhoneNumber,
                        Specialization = lawyer.Specialization.ToString(),
                        ProfileImageUrl = lawyer.ProfileImage?.ImageUrl,
                        Country = lawyer.Country,
                        Bio = lawyer.Bio,
                        Experiences = lawyer.Experiences,
                        Qualifications = lawyer.Qualifications,
                        YearsOfExperience = lawyer.YearsOfExperience,
                        LicenseNumber = lawyer.LicenseNumber,
                        BankName = lawyer.BankName,
                        LicenseDocument = lawyer.LicenseDocumentPath,
                        QualificationDocument = lawyer.QualificationDocumentPath,
                    };

                    return _responseHandler.Success<object>(lawyerResponse, "Profile fetched successfully");
                }
            }

            return _responseHandler.NotFound<object>("User not found as Client or Lawyer");
        }

        public async Task<Response<bool>> UpdateProfileAsync(UpdateClientProfileRequest dto)
        {
            var userId = GetUserIdClaim();
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized attempt to update profile (no UserId claim).");
                return _responseHandler.Unauthorized<bool>("You are not authorized to update this profile.");
            }

            var userExists = await UserExistsAsync(userId);
            if (!userExists)
            {
                _logger.LogWarning("UserId {UserId} not found.", userId);
                return _responseHandler.Unauthorized<bool>("You are not authorized to update this profile.");
            }

            var client = await _context.Clients
                .Where(c => !c.IsDeleted)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == userId);

            if (client == null)
            {
                _logger.LogWarning("Client with UserId {UserId} not found", userId);
                return _responseHandler.BadRequest<bool>("Client not found");
            }

            try
            {

                if (!string.IsNullOrEmpty(dto.FirstName))
                {
                    client.FirstName = dto.FirstName;
                }
                if (!string.IsNullOrEmpty(dto.LastName))
                {
                    client.LastName = dto.LastName;
                }
                if (!string.IsNullOrEmpty(dto.Address))
                {
                    client.Address = dto.Address;
                }
                if (dto.ProfileImage is not null)
                {
                    var uploadImageResult = await UpdateProfileImageAsync(dto.ProfileImage);
                    if (!uploadImageResult.Succeeded)
                    {
                        _logger.LogWarning("Failed to upload profile image for UserId {UserId}: {ErrorMessage}", userId, uploadImageResult.Message);
                        return _responseHandler.BadRequest<bool>("Failed to upload profile image: " + uploadImageResult.Message);
                    }
                }
                await _context.SaveChangesAsync();

                return _responseHandler.Success(true, "Profile updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for UserId {UserId}", userId);
                return _responseHandler.BadRequest<bool>("An error occurred while updating the profile");
            }
        }

        public async Task<Response<bool>> UpdateLawyerProfileAsync(UpdateLawyerProfileRequest dto)
        {
            var userId = GetUserIdClaim();
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized attempt to update lawyer profile (no UserId claim).");
                return _responseHandler.Unauthorized<bool>("You are not authorized to update this profile.");
            }

            var userExists = await UserExistsAsync(userId);
            if (!userExists)
            {
                _logger.LogWarning("UserId {UserId} not found.", userId);
                return _responseHandler.Unauthorized<bool>("You are not authorized to update this profile.");
            }

            var lawyer = await _context.Lawyers
                .Where(l => !l.IsDeleted)
                .Include(l => l.User)
                .Include(l => l.ProfileImage)
                .FirstOrDefaultAsync(l => l.Id == userId);

            if (lawyer == null)
            {
                _logger.LogWarning("Lawyer with UserId {UserId} not found", userId);
                return _responseHandler.BadRequest<bool>("Lawyer not found");
            }

            try
            {
                // Update only fields that are provided (not null)
                if (!string.IsNullOrEmpty(dto.FirstName))
                {
                    lawyer.FirstName = dto.FirstName;
                }

                if (!string.IsNullOrEmpty(dto.LastName))
                {
                    lawyer.LastName = dto.LastName;
                }

                if (!string.IsNullOrEmpty(dto.UserName))
                {
                    // Check if username is already taken by another user
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.UserName == dto.UserName && u.Id != userId);
                    
                    if (existingUser != null)
                    {
                        return _responseHandler.BadRequest<bool>("Username is already taken");
                    }

                    lawyer.User.UserName = dto.UserName;
                }

                if (!string.IsNullOrEmpty(dto.Bio))
                {
                    lawyer.Bio = dto.Bio;
                }

                if (!string.IsNullOrEmpty(dto.Experiences))
                {
                    lawyer.Experiences = dto.Experiences;
                }

                if (!string.IsNullOrEmpty(dto.Qualifications))
                {
                    lawyer.Qualifications = dto.Qualifications;
                }

                if (dto.YearsOfExperience.HasValue)
                {
                    lawyer.YearsOfExperience = dto.YearsOfExperience.Value;
                }

                if (dto.Age.HasValue)
                {
                    lawyer.Age = dto.Age.Value;
                }

                if (!string.IsNullOrEmpty(dto.Address))
                {
                    lawyer.Address = dto.Address;
                }

                if (dto.Specialization.HasValue)
                {
                    lawyer.Specialization = dto.Specialization.Value;
                }

                if (!string.IsNullOrEmpty(dto.Country))
                {
                    lawyer.Country = dto.Country;
                }

                if (!string.IsNullOrEmpty(dto.IBAN))
                {
                    lawyer.IBAN = dto.IBAN;
                }

                if (!string.IsNullOrEmpty(dto.BankAccountNumber))
                {
                    lawyer.BankAccountNumber = dto.BankAccountNumber;
                }

                if (!string.IsNullOrEmpty(dto.BankName))
                {
                    lawyer.BankName = dto.BankName;
                }

                // Handle profile image upload
                if (dto.ProfileImage != null)
                {
                    var uploadResult = await _imageUploadService.UploadAsync(dto.ProfileImage);
                    
                    if (uploadResult == null || string.IsNullOrEmpty(uploadResult.Url))
                    {
                        _logger.LogWarning("Failed to upload profile image for LawyerId {LawyerId}", userId);
                        return _responseHandler.BadRequest<bool>("Failed to upload profile image");
                    }

                    if (lawyer.ProfileImage == null)
                    {
                        lawyer.ProfileImage = new ProfileImage
                        {
                            ImageUrl = uploadResult.Url,
                            LawyerId = lawyer.Id
                        };
                    }
                    else
                    {
                        lawyer.ProfileImage.ImageUrl = uploadResult.Url;
                    }
                }

                lawyer.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Lawyer profile updated successfully for LawyerId {LawyerId}", userId);
                return _responseHandler.Success(true, "Lawyer profile updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lawyer profile for UserId {UserId}", userId);
                return _responseHandler.ServerError<bool>("An error occurred while updating the lawyer profile");
            }
        }

        public async Task<Response<bool>> UpdateProfileImageAsync(IFormFile profileImage)
        {
            var userId = GetUserIdClaim();
            if (string.IsNullOrEmpty(userId))
                return _responseHandler.Unauthorized<bool>("You are not authorized to update this profile image.");

            var client = await _context.Clients
                .Where(c => !c.IsDeleted)
                .Include(c => c.ProfileImage)
                .FirstOrDefaultAsync(c => c.Id == userId);

            if (client == null)
                return _responseHandler.BadRequest<bool>("Client not found");

            try
            {
                var uploadResult = await _imageUploadService.UploadAsync(profileImage);

                if (client.ProfileImage == null)
                {
                    client.ProfileImage = new ProfileImage
                    {
                        ImageUrl = uploadResult.Url,
                        ClientId = client.Id
                    };
                }
                else
                {
                    client.ProfileImage.ImageUrl = uploadResult.Url;
                }

                await _context.SaveChangesAsync();

                return _responseHandler.Success(true, "Profile image updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile image for UserId {UserId}", userId);
                return _responseHandler.ServerError<bool>("An error occurred while updating the profile image");
            }
        }

    }
}
