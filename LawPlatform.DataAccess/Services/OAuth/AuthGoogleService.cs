using Ecommerce.DataAccess.Services.OAuth;
using LawPlatform.Utilities.Configurations;
using LawPlatform.DataAccess.Services.Token;
using Google.Apis.Auth;
using LawPlatform.DataAccess.Services.Token;
using LawPlatform.Entities.DTO.Account.Auth.Register;
using LawPlatform.Entities.Models.Auth.Identity;
using LawPlatform.Entities.Models.Auth.Identity;
using LawPlatform.Entities.Shared.Bases;
using LawPlatform.Entities.Shared.Bases;
using LawPlatform.Utilities.Configurations;
using LawPlatform.Utilities.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LawPlatform.DataAccess.Services.OAuth
{
    public class AuthGoogleService : IAuthGoogleService
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenStoreService _tokenService;
        private readonly ResponseHandler _responseHandler;
        private readonly GoogleAuthSettings _settings;

        public AuthGoogleService(UserManager<User> userManager, ITokenStoreService tokenService, ResponseHandler responseHandler, IOptions<GoogleAuthSettings> options)
        {
            _userManager = userManager;
            _tokenService = tokenService;

            _responseHandler = responseHandler;
            _settings = options.Value;
        }

        public async Task<Entities.Shared.Bases.Response<GoogleRegisterResponse>> AuthenticateWithGoogleAsync(string idToken)
        {
            try
            {
                // Validate the ID token and retrieve the payload
                var payload = await ValidateGoogleTokenAsync(idToken);

                if (payload == null)
                    throw new UnauthorizedAccessException("Invalid Google token");

                // Check if the user already exists by email
                var user = await _userManager.Users.FirstOrDefaultAsync(user => user.Email == payload.Email);

                // If user doesn't exist, create a new user
                if (user == null)
                {
                    user = new User
                    {
                        UserName = payload.Email.Split('@')[0],
                        Email = payload.Email,
                        PhoneNumber = "N/A"
                    };

                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        throw new UserCreationException($"Failed to create user: {errors}");
                    }

                    await _userManager.AddToRoleAsync(user, "USER");
                    // Generate token for the user
                    var userTokens = await _tokenService.GenerateAndStoreTokensAsync(user.Id, user);

                    var userRoles = await _userManager.GetRolesAsync(user);
                    var response = new GoogleRegisterResponse
                    {
                        UserId = user.Id,
                        Roles = userRoles.FirstOrDefault(),
                        AccessToken = userTokens.AccessToken,
                        RefreshToken = userTokens.RefreshToken,
                        UserName = payload.Email.Split('@')[0],
                        Email = payload.Email,
                    };
                    return _responseHandler.Success(response, "Login successful.");

                }
                else
                {
                    //if user != null its mean the user is found or already signed in with google
                    // Generate token for the user
                    var userTokens = await _tokenService.GenerateAndStoreTokensAsync(user.Id, user);

                    var userRoles = await _userManager.GetRolesAsync(user);
                    var response = new GoogleRegisterResponse
                    {
                        UserId = user.Id,
                        Roles = userRoles.FirstOrDefault(),
                        AccessToken = userTokens.AccessToken,
                        RefreshToken = userTokens.RefreshToken,
                        UserName = payload.Email.Split('@')[0],
                        Email = payload.Email
                    };
                    return _responseHandler.Success(response, "Login successful.");
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (UserCreationException)
            {
                throw; // To know when exception thrown from and when ?
            }
            catch (Exception ex)
            {
                throw new Exception($"Google authentication failed: {ex.Message}", ex);
            }
        }

        public async Task<GoogleJsonWebSignature.Payload> ValidateGoogleTokenAsync(string idToken)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

                if (payload == null || string.IsNullOrEmpty(payload.Email))
                    throw new UnauthorizedAccessException("Invalid Google Token: Payload is null or missing email");

                return payload;
            }
            catch (Google.Apis.Auth.InvalidJwtException ex)
            {
                throw new UnauthorizedAccessException("Invalid Google Token: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to validate Google Token: " + ex.Message, ex);
            }
        }
    }
}
