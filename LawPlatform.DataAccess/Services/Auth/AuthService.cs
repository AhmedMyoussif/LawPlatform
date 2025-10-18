using System.Security.Claims;
using LawPlatform.Entities.DTO.Account.Auth.ResetPassword;
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
using LawPlatform.DataAccess.Services.Notification;

namespace LawPlatform.DataAccess.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly LawPlatformContext _context;
        private readonly IEmailService _emailService;
        private readonly ResponseHandler _responseHandler;
        private readonly IImageUploadService _imageUploadService;
        private readonly ITokenStoreService _tokenStoreService;
        private readonly ILogger<AuthService> _logger;
        private readonly INotificationService _notificationService;
        public AuthService(
            UserManager<User> userManager,
            LawPlatformContext context,
            IEmailService emailService,
            ResponseHandler responseHandler,
            IImageUploadService imageUploadService,
            ITokenStoreService tokenStoreService,
            ILogger<AuthService> logger,
            INotificationService notificationService)
        {
            _userManager = userManager;
            _context = context;
            _emailService = emailService;
            _responseHandler = responseHandler;
            _imageUploadService = imageUploadService;
            _tokenStoreService = tokenStoreService;
            _logger = logger;
            _notificationService = notificationService;
        }

        #region Login
        public async Task<Response<LoginResponse>> LoginAsync(LoginRequest model)
        {
            try
            {
                var user = await FindUserByEmailAsync(model.Email);
                if (user == null)
                    return _responseHandler.NotFound<LoginResponse>("User not found.");

                if (!await _userManager.CheckPasswordAsync(user, model.Password))
                    return _responseHandler.BadRequest<LoginResponse>("Invalid password.");

                if (!user.EmailConfirmed)
                    return _responseHandler.BadRequest<LoginResponse>("Email is not verified. Please verify your email first.");

                var roles = await _userManager.GetRolesAsync(user);

                // Generate tokens and store refresh token associated with user.Id
                var tokens = await _tokenStoreService.GenerateAndStoreTokensAsync(user);

                var response = new LoginResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = roles.FirstOrDefault(),
                    IsEmailConfirmed = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber,
                    AccessToken = tokens.AccessToken,
                    RefreshToken = tokens.RefreshToken
                };

                return _responseHandler.Success(response, "Login successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LoginAsync");
                return _responseHandler.ServerError<LoginResponse>("An error occurred while logging in.");
            }
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
                    UserName = model.FirstName,
                    Email = model.Email.Trim().ToLower(),
                    PhoneNumber = model.PhoneNumber,
                    EmailConfirmed = true
                };

                var createUserResult = await _userManager.CreateAsync(user, model.Password);
                if (!createUserResult.Succeeded)
                {
                    var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("User creation failed for Email: {Email}. Errors: {Errors}", model.Email, errors);
                    return _responseHandler.BadRequest<CustomerRegisterResponse>(errors);
                }

                await _userManager.AddToRoleAsync(user, "Client");

                var client = new Client
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    User = user,
                    Id = user.Id,              // Shared PK with User
                    CreatedAt = DateTime.UtcNow,
                    Address = model.Address
                };

                _context.Clients.Add(client);
                await _context.SaveChangesAsync();

                await _emailService.SendClientRegstrationEmailAsync(user);

                // generate & store tokens (stored with user.Id)
                var tokens = await _tokenStoreService.GenerateAndStoreTokensAsync(user);

                await transaction.CommitAsync();

                var response = new CustomerRegisterResponse
                {
                    Id = client.Id,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = user.UserName,
                    Role = "Client",
                    IsEmailConfirmed = true,
                    AccessToken = tokens.AccessToken,
                    RefreshToken = tokens.RefreshToken,
                    Address = model.Address
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
            bool committed = false;

            Lawyer? lawyer = null;
            User? user = null;
            LawyerRegisterResponse? response = null;

            try
            {
                user = new User
                {
                    UserName = model.UserName.Trim(),
                    Email = model.Email.Trim().ToLower(),
                    PhoneNumber = model.PhoneNumber,
                    EmailConfirmed = true
                };

                var createUserResult = await _userManager.CreateAsync(user, model.Password);
                if (!createUserResult.Succeeded)
                {
                    var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("User creation failed for Email: {Email}. Errors: {Errors}", model.Email, errors);
                    return _responseHandler.BadRequest<LawyerRegisterResponse>(errors);
                }

                await _userManager.AddToRoleAsync(user, "Lawyer");

                // Upload files...
                string? qualificationDocumentUrl = null;
                if (model.QualificationDocument != null)
                {
                    var uploadResult = await _imageUploadService.UploadAsync(model.QualificationDocument);
                    qualificationDocumentUrl = uploadResult?.Url;
                }

                if (model.LicenseDocument == null)
                {
                    return _responseHandler.BadRequest<LawyerRegisterResponse>("License document is required.");
                }

                var licenseUploadResult = await _imageUploadService.UploadAsync(model.LicenseDocument);
                if (licenseUploadResult == null || string.IsNullOrEmpty(licenseUploadResult.Url))
                {
                    return _responseHandler.BadRequest<LawyerRegisterResponse>("Failed to upload license document.");
                }
                string licenseDocumentUrl = licenseUploadResult.Url;

                // Create Lawyer
                lawyer = new Lawyer
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    User = user,
                    Id = user.Id,
                    Bio = model.Bio,
                    Experiences = model.Experiences,
                    Qualifications = model.Qualifications,
                    YearsOfExperience = model.YearsOfExperience,
                    LicenseNumber = model.LicenseNumber,
                    LicenseDocumentPath = licenseDocumentUrl,
                    QualificationDocumentPath = qualificationDocumentUrl,
                    Specialization = model.Specialization,
                    Country = model.Country,
                    IBAN = model.IBAN,
                    Address = model.Address,
                    Age = model.Age,
                    BankAccountNumber = model.BankAccountNumber,
                    BankName = model.BankName,
                    Status = ApprovalStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Lawyers.AddAsync(lawyer);
                await _context.SaveChangesAsync();

                var tokens = await _tokenStoreService.GenerateAndStoreTokensAsync(user);

                // Commit transaction BEFORE sending email/notification
                await transaction.CommitAsync();
                committed = true;

                response = new LawyerRegisterResponse
                {
                    Id = lawyer.Id,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    UserName = user.UserName,
                    FullName = $"{lawyer.FirstName} {lawyer.LastName}",
                    Role = "Lawyer",
                    Specialization = lawyer.Specialization.ToString(),
                    LicenseNumber = lawyer.LicenseNumber,
                    LicenseDocumentPath = lawyer.LicenseDocumentPath,
                    QualificationDocumentPath = lawyer.QualificationDocumentPath,
                    Status = lawyer.Status,
                    AccessToken = tokens.AccessToken,
                    RefreshToken = tokens.RefreshToken,
                    Address = lawyer.Address,
                    Age = lawyer.Age
                };
            }
            catch (Exception ex)
            {
                // Rollback only if we didn't commit
                if (!committed)
                {
                    try
                    {
                        await transaction.RollbackAsync();
                    }
                    catch (Exception rollEx)
                    {
                        _logger.LogWarning(rollEx, "Rollback warning (transaction may be already completed).");
                    }
                }

                _logger.LogError(ex, "Error occurred during RegisterLawyerAsync for Email: {Email}", model.Email);
                return _responseHandler.BadRequest<LawyerRegisterResponse>("An error occurred during registration.");
            }

            // ===== Send email & notification AFTER transaction committed =====
            try
            {
                if (lawyer != null)
                {
                    // If your Email service expects User, pass lawyer.User; else we assume it accepts Lawyer
                    await _emailService.SendLawyerEmailAsync(lawyer, LawyerEmailType.Pending);
                }

                await _notificationService.NotifyUserAsync(lawyer!.Id,
                    "Pending Approval",
                    "Your lawyer account is pending approval by the admin.");
            }
            catch (Exception postEx)
            {
                _logger.LogError(postEx, "Failed to send post-registration email/notification for LawyerId {LawyerId}", lawyer?.Id);
            }

            return _responseHandler.Created(response!, "Lawyer registered successfully and is pending admin approval.");
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

            // optionally send OTP via _emailService
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

            // invalidate old tokens by user.Id
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
        #endregion

        #region Change / Logout
        public async Task<Response<string>> ChangePasswordAsync(ClaimsPrincipal userClaims, ChangePasswordRequest request)
        {
            try
            {
                var userId = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return _responseHandler.Unauthorized<string>("User not authenticated");

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return _responseHandler.NotFound<string>("User not found");

                var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
                if (!isCurrentPasswordValid)
                    return _responseHandler.BadRequest<string>("Current password is incorrect");

                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return _responseHandler.BadRequest<string>(errors);
                }

                // invalidate tokens for this user
                await _tokenStoreService.InvalidateOldTokensAsync(user.Id);

                return _responseHandler.Success<string>(null, "Password changed successfully. Please login again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ChangePasswordAsync");
                return _responseHandler.ServerError<string>($"An error occurred while changing password: {ex.Message}");
            }
        }

        public async Task<Response<string>> LogoutAsync(ClaimsPrincipal userClaims)
        {
            try
            {
                var userId = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return _responseHandler.Unauthorized<string>("User not authenticated");

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return _responseHandler.NotFound<string>("User not found");

                // invalidate tokens for this user
                await _tokenStoreService.InvalidateOldTokensAsync(user.Id);

                return _responseHandler.Success<string>(null, "Logged out successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LogoutAsync");
                return _responseHandler.ServerError<string>($"An error occurred during logout: {ex.Message}");
            }
        }
        #endregion

        #region Refresh Token
        public async Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken)
        {
            _logger.LogInformation("Starting RefreshTokenAsync for token: {TokenSnippet}", refreshToken.Substring(0, Math.Min(8, refreshToken.Length)));

            var isValid = await _tokenStoreService.IsValidAsync(refreshToken);
            if (!isValid)
            {
                _logger.LogWarning("Invalid refresh token provided: {TokenSnippet}", refreshToken.Substring(0, Math.Min(8, refreshToken.Length)));
                throw new SecurityTokenException("Invalid refresh token");
            }

            var tokenEntry = await _context.UserRefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken);
            if (tokenEntry == null)
            {
                _logger.LogWarning("No refresh token entry found for token: {TokenSnippet}", refreshToken.Substring(0, Math.Min(8, refreshToken.Length)));
                throw new SecurityTokenException("Invalid refresh token");
            }

            var user = await _userManager.FindByIdAsync(tokenEntry.UserId);
            if (user == null)
            {
                _logger.LogWarning("User not found for refresh token with UserId: {UserId}", tokenEntry.UserId);
                throw new SecurityTokenException("Invalid user");
            }

            // invalidate old tokens for this user
            await _tokenStoreService.InvalidateOldTokensAsync(user.Id);

            // generate & store new tokens (saved with user.Id)
            var userTokens = await _tokenStoreService.GenerateAndStoreTokensAsync(user);

            return new RefreshTokenResponse
            {
                AccessToken = userTokens.AccessToken,
                RefreshToken = userTokens.RefreshToken
            };
        }
        #endregion

        #region Helpers
        private async Task<string?> CheckIfEmailOrPhoneExists(string email, string? phoneNumber)
        {
            if (await _userManager.FindByEmailAsync(email.Trim().ToLower()) != null)
                return "Email is already registered.";
            if (!string.IsNullOrEmpty(phoneNumber) && await _userManager.Users.AnyAsync(u => u.PhoneNumber == phoneNumber))
                return "Phone number is already registered.";
            return null;
        }

        private async Task<User?> FindUserByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email)) return null;
            return await _userManager.FindByEmailAsync(email.Trim().ToLower());
        }
        #endregion
    }
}
