using LawPlatform.DataAccess.Services.OAuth;
using LawPlatform.Entities.DTO.Account.Auth.ResetPassword;
using FluentValidation;
using LawPlatform.DataAccess.Services.Auth;
using LawPlatform.DataAccess.Services.Token;
using LawPlatform.Entities.DTO.Account.Auth;
using LawPlatform.Entities.DTO.Account.Auth.Admin;
using LawPlatform.Entities.DTO.Account.Auth.Login;
using LawPlatform.Entities.DTO.Account.Auth.Register;
using LawPlatform.Entities.DTO.Account.Auth.ResetPassword;
using LawPlatform.Entities.Shared.Bases;
using LawPlatform.Utilities.Enums;
using LawPlatform.Utilities.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using LoginRequest = LawPlatform.Entities.DTO.Account.Auth.Login.LoginRequest;
using ResetPasswordRequest = LawPlatform.Entities.DTO.Account.Auth.ResetPassword.ResetPasswordRequest;
using LawPlatform.DataAccess.Services.OAuth;

namespace LawPlatform.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenStoreService _tokenStoreService;
        private readonly ResponseHandler _responseHandler;      
        private readonly IValidator<LoginRequest> _loginValidator;
        private readonly IValidator<ForgetPasswordRequest> _forgetPasswordValidator;
        private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;
        private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;
        private readonly IAuthGoogleService _authGoogleService;
        public AccountController(IAuthService authService, ResponseHandler responseHandler, IValidator<LoginRequest> loginValidator, IValidator<ForgetPasswordRequest> forgetPasswordValidator, IValidator<ResetPasswordRequest> resetPasswordValidator ,IAuthGoogleService authGoogleService, IValidator<ChangePasswordRequest> changePasswordValidator)
        {
            _authService = authService;
            _responseHandler = responseHandler;
           
            _loginValidator = loginValidator;
            _forgetPasswordValidator = forgetPasswordValidator;
            _resetPasswordValidator = resetPasswordValidator;
            _authGoogleService = authGoogleService;
            _changePasswordValidator = changePasswordValidator;
        }

        #region Login
        [HttpPost("login")]
        public async Task<ActionResult<Response<LoginResponse>>> Login([FromBody] LoginRequest model)
        {
            var result = await _authService.LoginAsync(model);
            if (!result.Succeeded) return BadRequest(result);
            Response.Cookies.Append("jwt", result.Data.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddMinutes(30) // Set the expiration time as needed
            });
            return Ok(result);
        }
        
        
        [HttpPost("login/google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest googleLoginDto)
        {
            if (!ModelState.IsValid)
                return _responseHandler.HandleModelStateErrors(ModelState);

            try
            {
                var token = await _authGoogleService.AuthenticateWithGoogleAsync(googleLoginDto.IdToken);
                var response = _responseHandler.Success(token, "Logged in with Google successfully");
                return StatusCode((int)response.StatusCode, response);
            }
            catch (UnauthorizedAccessException ex)  // When 'IdToke' is not valid
            {
                var response = _responseHandler.Unauthorized<string>("Google authentication failed: " + ex.Message);
                return StatusCode((int)response.StatusCode, response);
            }
            catch (UserCreationException ex)  // When user creation faild
            {
                var response = _responseHandler.InternalServerError<string>("User creation failed: " + ex.Message);
                return StatusCode((int)response.StatusCode, response);
            }
            catch (Exception ex)  // Server error
            {
                var response = _responseHandler.ServerError<string>("An error occurred: " + ex.Message);
                return StatusCode((int)response.StatusCode, response);
            }
        }

        #endregion

        #region Register Client
        [HttpPost("register/Client")]
        public async Task<ActionResult<Response<CustomerRegisterResponse>>> RegisterCustomer([FromBody] CustomerRegisterRequest model)
        {
            var result = await _authService.RegisterCustomerAsync(model);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }
        #endregion

        #region Register Lawyer
        [HttpPost("register/lawyer")]
        public async Task<ActionResult<Response<LawyerRegisterResponse>>> RegisterLawyer([FromForm] LawyerRegisterRequest model)
        {
            var result = await _authService.RegisterLawyerAsync(model);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }
        #endregion

        #region Forgot Password
        [HttpPost("forgot-password")]
        public async Task<ActionResult<Response<ForgetPasswordResponse>>> ForgotPassword([FromBody] ForgetPasswordRequest model)
        {
            var result = await _authService.ForgotPasswordAsync(model);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }
        #endregion

        #region Reset Password
        [HttpPost("reset-password")]
        public async Task<ActionResult<Response<ResetPasswordResponse>>> ResetPassword([FromBody] ResetPasswordRequest model)
        {
            var result = await _authService.ResetPasswordAsync(model);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }
        #endregion

        #region RefreshToken

        
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return BadRequest(_responseHandler.BadRequest<string>("RefreshTokenIsNotFound"));
            try
            {
                var newTokens = await _authService.RefreshTokenAsync(refreshToken);

                return Ok(_responseHandler.Success<RefreshTokenResponse>(newTokens, "User token refreshed succsessfully"));
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(_responseHandler.Unauthorized<string>(ex.Message));
            }
            catch (Exception ex)
            {
                var error = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    _responseHandler.BadRequest<string>("UnexpectedError" + ": " + error)
                );
            }
        }

        #endregion

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var validationResult = await _changePasswordValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return BadRequest(_responseHandler.BadRequest<object>(errors));
            }

            var response = await _authService.ChangePasswordAsync(User, request);
            return StatusCode((int)response.StatusCode, response);
        }
        
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var response = await _authService.LogoutAsync(User);
            Response.Cookies.Delete("jwt");
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
