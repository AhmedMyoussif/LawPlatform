using System.Security.Claims;
using Ecommerce.Entities.DTO.Account.Auth.ResetPassword;
using LawPlatform.DataAccess.ApplicationContext;
using LawPlatform.DataAccess.Services.Email;
using LawPlatform.DataAccess.Services.ImageUploading;
using LawPlatform.DataAccess.Services.Token;
using LawPlatform.Entities.DTO.Account.Auth;
using LawPlatform.Entities.DTO.Account.Auth.Admin;
using LawPlatform.Entities.DTO.Account.Auth.Login;
using LawPlatform.Entities.DTO.Account.Auth.Register;
using LawPlatform.Entities.DTO.Account.Auth.ResetPassword;
using LawPlatform.Entities.Models.Auth.Identity;
using LawPlatform.Entities.Models.Auth.Users;
using LawPlatform.Entities.Shared.Bases;
using LawPlatform.Utilities.Enums;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace LawPlatform.DataAccess.Services.Auth
{
    // public static class Roles
    // {
    //     public const string Buyer = "Cleint";
    //     public const string Lawyer = "Lawyer";
    // }

    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly LawPlatformContext _context;
        private readonly IEmailService _emailService;
        private readonly ResponseHandler _responseHandler;
        private readonly IImageUploadService _cloudinary;
        private readonly ITokenStoreService _tokenStoreService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<User> userManager,
            LawPlatformContext context,
            IEmailService emailService,
            ResponseHandler responseHandler,
            IImageUploadService cloudinary,
            ITokenStoreService tokenStoreService,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _context = context;
            _emailService = emailService;
            _responseHandler = responseHandler;
            _cloudinary = cloudinary;
            _tokenStoreService = tokenStoreService;
            _logger = logger;
        }

        #region Login
        public async Task<Response<LoginResponse>> LoginAsync(LoginRequest model)
        {
            var user = await FindUserByEmailAsync(model.Email);
            if (user == null)
                return _responseHandler.NotFound<LoginResponse>("User not found.");

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
                return _responseHandler.BadRequest<LoginResponse>("Invalid password.");

            if (!user.EmailConfirmed)
                return _responseHandler.BadRequest<LoginResponse>("Email is not verified. Please verify your email first.");

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Lawyer"))
            {
                var lawyer = await _context.Lawyers.FirstOrDefaultAsync(l => l.Id == user.Id);
                if (lawyer == null || lawyer.Status != ApprovalStatus.Approved)
                    return _responseHandler.BadRequest<LoginResponse>("Lawyer account is not approved by admin.");
            }

            var tokens = await GenerateAndStoreTokensAsync(user.Id, user);

            var response = new LoginResponse
            {
                Id = user.Id,
                Email = user.Email,
                Role = roles.FirstOrDefault(),
                IsEmailConfirmed = user.EmailConfirmed,
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken
            };

            return _responseHandler.Success(response, "Login successful.");
        }

        #endregion

        #region Register Client
        public async Task<Response<CustomerRegisterResponse>> RegisterCustomerAsync(CustomerRegisterRequest model)
        {
            _logger.LogInformation("RegisterCustomerAsync started for Email: {Email}", model.Email);

            var emailPhoneCheck = await CheckIfEmailOrPhoneExists(model.Email, model.PhoneNumber);
            if (emailPhoneCheck != null)
                return _responseHandler.BadRequest<CustomerRegisterResponse>(emailPhoneCheck);
         
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = new User
                {
                    UserName = model.Email.Trim().ToLower(),
                    Email = model.Email.Trim().ToLower(),
                    PhoneNumber = model.PhoneNumber,
                };

                var createUserResult = await _userManager.CreateAsync(user, model.Password);
                if (!createUserResult.Succeeded)
                {
                    var errors = createUserResult.Errors.Select(e => e.Description).ToList();
                    _logger.LogWarning("User creation failed for Email: {Email}. Errors: {Errors}", model.Email, string.Join(", ", errors));
                    return _responseHandler.BadRequest<CustomerRegisterResponse>(string.Join(", ", errors));;
                }

                await _userManager.AddToRoleAsync(user, "Client");
                await _userManager.CreateAsync(user);

                // var client = new Client
                // {
                //     Id = user.Id,
                //    FirstName = model.FirstName,
                //    LastName = model.LastName,
                //    BirthDate = model.BirthDate,
                //
                // };
                // _context.Clients.Add(client);

                var tokens = await GenerateAndStoreTokensAsync(user.Id, user);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = new CustomerRegisterResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Role = "Client",
                    IsEmailConfirmed = true,
                    AccessToken = tokens.AccessToken,
                    RefreshToken = tokens.RefreshToken
                };

                return _responseHandler.Created(response, "Client registered successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred during RegisterCustomerAsync for Email: {Email}", model.Email);
                return _responseHandler.BadRequest<CustomerRegisterResponse>("An error occurred during registration.");
            }
        }
        #endregion

        #region Register Lawyer
        public async Task<Response<LawyerRegisterResponse>> RegisterLawyerAsync(LawyerRegisterRequest model)
        {
            _logger.LogInformation("RegisterLawyerAsync started for Email: {Email}", model.Email);

            var emailPhoneCheck = await CheckIfEmailOrPhoneExists(model.Email, model.PhoneNumber);
            if (emailPhoneCheck != null)
                return _responseHandler.BadRequest<LawyerRegisterResponse>(emailPhoneCheck);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = new User
                {
                    UserName = model.Email.Trim().ToLower(),
                    Email = model.Email.Trim().ToLower(),
                    PhoneNumber = model.PhoneNumber
                };

                var createUserResult = await _userManager.CreateAsync(user, model.Password);
                if (!createUserResult.Succeeded)
                {
                    var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                    return _responseHandler.BadRequest<LawyerRegisterResponse>(errors);
                }

                await _userManager.AddToRoleAsync(user, "Lawyer");

                var names = model.FullName?.Split(' ', 2);
                var firstName = names != null && names.Length > 0 ? names[0] : "Unknown";
                var lastName = names != null && names.Length > 1 ? names[1] : "Unknown";

                string? qualificationDocumentUrl = null;
                if (model.QualificationDocument != null)
                {
                    var uploadResult = await _cloudinary.UploadAsync(model.QualificationDocument);
                    qualificationDocumentUrl = uploadResult.Url;
                }
                
                await _userManager.CreateAsync(user);

                // var lawyer = new Lawyer
                // {
                //     Id = user.Id,
                //     FirstName = firstName,
                //     LastName = lastName,
                //     BankAccountNumber = model.BankAccountNumber,
                //     BankName = model.BankName,
                //     Country = model.Country,
                //     Bio = model.Bio,
                //     Experiences = model.Experiences,
                //     QualificationDocumentPath = qualificationDocumentUrl,
                //     YearsOfExperience = model.YearsOfExperience,
                //     Qualifications = model.Qualifications,
                //     Status = ApprovalStatus.Pending,
                //     CreatedAt = DateTime.UtcNow,
                // };

              

                var tokens = await GenerateAndStoreTokensAsync(user.Id, user);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = new LawyerRegisterResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    FullName = model.FullName,
                    Role = "Lawyer",
                    Status = model.Status.ToString(),
                    QualificationDocumentPath = qualificationDocumentUrl,
                    AccessToken = tokens.AccessToken,
                    RefreshToken = tokens.RefreshToken
                };

                return _responseHandler.Created(response, "Lawyer registered successfully and is pending admin approval.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred during RegisterLawyerAsync for Email: {Email}", model.Email);
                return _responseHandler.BadRequest<LawyerRegisterResponse>("An error occurred during registration.");
            }
        }
        #endregion

        #region Forgot / Reset Password
        public async Task<Response<ForgetPasswordResponse>> ForgotPasswordAsync(ForgetPasswordRequest model)
        {
            var user = await FindUserByEmailAsync(model.Email);
            if (user == null)
                return _responseHandler.NotFound<ForgetPasswordResponse>("User not found.");

            var response = new ForgetPasswordResponse
            {
                UserId = user.Id
            };

            return _responseHandler.Success(response, "OTP sent to your email. Please use it to reset your password.");
        }

        public async Task<Response<ResetPasswordResponse>> ResetPasswordAsync(ResetPasswordRequest model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return _responseHandler.NotFound<ResetPasswordResponse>("User not found.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return _responseHandler.BadRequest<ResetPasswordResponse>(errors);
            }

            await _tokenStoreService.InvalidateOldTokensAsync(user.Id);

            var roles = await _userManager.GetRolesAsync(user);
            var response = new ResetPasswordResponse
            {
                UserId = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = roles.FirstOrDefault()
            };

            return _responseHandler.Success(response, "Password reset successfully. Please log in with your new password.");
        }
        
        public async Task<Response<string>> ChangePasswordAsync(ClaimsPrincipal userClaims, ChangePasswordRequest request)
        {
            try
            {
                // Get user ID from claims
                var userId = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return _responseHandler.Unauthorized<string>("User not authenticated");
                }

                // Find user
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return _responseHandler.NotFound<string>("User not found");
                }

                // Verify current password
                var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
                if (!isCurrentPasswordValid)
                {
                    return _responseHandler.BadRequest<string>("Current password is incorrect");
                }

                // Change password
                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return _responseHandler.BadRequest<string>(errors);
                }

                // Invalidate all existing refresh tokens for security
                await _tokenStoreService.InvalidateOldTokensAsync(userId);

                return _responseHandler.Success<string>(null,"Password changed successfully. Please login again.");
            }
            catch (Exception ex)
            {
                return _responseHandler.ServerError<string>($"An error occurred while changing password: {ex.Message}");
            }
        }
        
        public async Task<Response<string>> LogoutAsync(ClaimsPrincipal userClaims)
        {
            try
            {
                // Get user ID from claims
                var userId = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return _responseHandler.Unauthorized<string>("User not authenticated");
                }

                // Invalidate all refresh tokens for this user
                await _tokenStoreService.InvalidateOldTokensAsync(userId);

                return _responseHandler.Success<string>(null,"Logged out successfully");
            }
            catch (Exception ex)
            {
                return _responseHandler.ServerError<string>($"An error occurred during logout: {ex.Message}");
            }
        }
        #endregion
        
        
        public async Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken)
        {
            _logger.LogInformation("Starting RefreshTokenAsync for token: {TokenSnippet}", refreshToken.Substring(0, Math.Min(8, refreshToken.Length)));

            try
            {
                var isValid = await _tokenStoreService.IsValidAsync(refreshToken);
                if (!isValid)
                {
                    _logger.LogWarning("Invalid refresh token provided: {TokenSnippet}", refreshToken.Substring(0, Math.Min(8, refreshToken.Length)));
                    throw new SecurityTokenException("Invalid refresh token");
                }

                var tokenEntry = await _context.UserRefreshTokens
                    .FirstOrDefaultAsync(r => r.Token == refreshToken);
                if (tokenEntry == null)
                {
                    _logger.LogWarning("No refresh token entry found for token: {TokenSnippet}", refreshToken.Substring(0, Math.Min(8, refreshToken.Length)));
                    throw new SecurityTokenException("Invalid refresh token");
                }

                var user = await _userManager.FindByIdAsync(tokenEntry.UserId.ToString());
                if (user == null)
                {
                    _logger.LogWarning("User not found for refresh token with UserId: {UserId}", tokenEntry.UserId);
                    throw new SecurityTokenException("Invalid user");
                }

                _logger.LogInformation("Invalidating old refresh tokens for user: {UserId}", user.Id);
                await _tokenStoreService.InvalidateOldTokensAsync(user.Id);

                _logger.LogInformation("Generating new access and refresh tokens for user: {UserId}", user.Id);
                var userTokens = await _tokenStoreService.GenerateAndStoreTokensAsync(user.Id, user);
                
                await _tokenStoreService.SaveRefreshTokenAsync(user.Id, userTokens.RefreshToken);
                _logger.LogInformation("New refresh token saved for user: {UserId}", user.Id);

                return new RefreshTokenResponse
                {
                    AccessToken = userTokens.AccessToken,
                    RefreshToken = userTokens.RefreshToken,
                };
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogError(ex, "Security token error during refresh token process for token: {TokenSnippet}", refreshToken.Substring(0, Math.Min(8, refreshToken.Length)));
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during refresh token process for token: {TokenSnippet}", refreshToken.Substring(0, Math.Min(8, refreshToken.Length)));
                throw;
            }
        }



        #region Helpers
        private async Task<string?> CheckIfEmailOrPhoneExists(string email, string? phoneNumber)
        {
            if (await _userManager.FindByEmailAsync(email.Trim().ToLower()) != null)
                return "Email is already registered.";
            if (!string.IsNullOrEmpty(phoneNumber) && await _userManager.Users.AnyAsync(u => u.PhoneNumber == phoneNumber))
                return "Phone number is already registered.";
            return null;
        }

        private async Task<(string AccessToken, string RefreshToken)> GenerateAndStoreTokensAsync(string userId, User user)
        {
            var accessToken = await _tokenStoreService.CreateAccessTokenAsync(user);
            var refreshToken = _tokenStoreService.GenerateRefreshToken();
            await _tokenStoreService.SaveRefreshTokenAsync(userId, refreshToken);
            return (accessToken, refreshToken);
        }

        private async Task<User?> FindUserByEmailAsync(string email)
        {
            if (!string.IsNullOrEmpty(email))
                return await _userManager.FindByEmailAsync(email.Trim().ToLower());

            return null;
        }
        #endregion
    }
}
