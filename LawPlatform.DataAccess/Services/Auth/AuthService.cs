using LawPlatform.DataAccess.ApplicationContext;
using LawPlatform.DataAccess.Services.Email;
using LawPlatform.DataAccess.Services.ImageUploading;
using LawPlatform.DataAccess.Services.Token;
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
    public static class Roles
    {
        public const string Buyer = "Cleint";
        public const string Lawyer = "Lawyer";
    }

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

            if (roles.Contains(Roles.Lawyer))
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
                Role = roles.FirstOrDefault() ?? Roles.Buyer,
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
                    var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                    return _responseHandler.BadRequest<CustomerRegisterResponse>(errors);
                }

                await _userManager.AddToRoleAsync(user, "Client");

                var client = new Client
                {
                    Id = user.Id,
                   FirstName = model.FirstName,
                   LastName = model.LastName,
                   BirthDate = model.BirthDate,

                };
                _context.Clients.Add(client);

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
                    Role = Roles.Buyer,
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

                await _userManager.AddToRoleAsync(user, Roles.Lawyer);

                var names = model.FullName?.Split(' ', 2);
                var firstName = names != null && names.Length > 0 ? names[0] : "Unknown";
                var lastName = names != null && names.Length > 1 ? names[1] : "Unknown";

                string? qualificationDocumentUrl = null;
                if (model.QualificationDocument != null)
                {
                    var uploadResult = await _cloudinary.UploadAsync(model.QualificationDocument);
                    qualificationDocumentUrl = uploadResult.Url;
                }

                var lawyer = new Lawyer
                {
                    Id = user.Id,
                    FirstName = firstName,
                    LastName = lastName,
                    BankAccountNumber = model.BankAccountNumber,
                    BankName = model.BankName,
                    Country = model.Country,
                    Bio = model.Bio,
                    Experiences = model.Experiences,
                    QualificationDocumentPath = qualificationDocumentUrl,
                    YearsOfExperience = model.YearsOfExperience,
                    Qualifications = model.Qualifications,
                    Status = ApprovalStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                };

                _context.Lawyers.Add(lawyer);

                var tokens = await GenerateAndStoreTokensAsync(user.Id, user);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = new LawyerRegisterResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    FullName = model.FullName,
                    Role = Roles.Lawyer,
                    Status = lawyer.Status,
                    QualificationDocumentUrl = lawyer.QualificationDocumentPath,
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
                Role = roles.FirstOrDefault() ?? Roles.Buyer
            };

            return _responseHandler.Success(response, "Password reset successfully. Please log in with your new password.");
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
