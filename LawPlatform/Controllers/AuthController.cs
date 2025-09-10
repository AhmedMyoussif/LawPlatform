using LawPlatform.DataAccess.Services.Auth;
using LawPlatform.Entities.DTO.Account.Auth.Admin;
using LawPlatform.Entities.DTO.Account.Auth.Login;
using LawPlatform.Entities.DTO.Account.Auth.Register;
using LawPlatform.Entities.DTO.Account.Auth.ResetPassword;
using LawPlatform.Entities.Shared.Bases;
using LawPlatform.Utilities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LawPlatform.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        #region Login
        [HttpPost("login")]
        public async Task<ActionResult<Response<LoginResponse>>> Login([FromBody] LoginRequest model)
        {
            var result = await _authService.LoginAsync(model);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }
        #endregion

        #region Register Customer
        [HttpPost("register/customer")]
        public async Task<ActionResult<Response<CustomerRegisterResponse>>> RegisterCustomer([FromBody] CustomerRegisterRequest model)
        {
            var result = await _authService.RegisterCustomerAsync(model);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }
        #endregion

        #region Register Lawyer
        [HttpPost("register/lawyer")]
        public async Task<ActionResult<Response<LawyerRegisterResponse>>> RegisterLawyer([FromBody] LawyerRegisterRequest model)
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

       
    }
}
