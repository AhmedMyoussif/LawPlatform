//using LawPlatform.Entities.DTO.Account.Auth.Admin;

using System.Security.Claims;
using LawPlatform.Entities.DTO.Account.Auth.ResetPassword;
using LawPlatform.Entities.DTO.Account.Auth;
using LawPlatform.Entities.DTO.Account.Auth.Admin;
using LawPlatform.Entities.DTO.Account.Auth.Login;
using LawPlatform.Entities.DTO.Account.Auth.Register;
using LawPlatform.Entities.DTO.Account.Auth.ResetPassword;
using LawPlatform.Entities.Shared.Bases;
using LawPlatform.Utilities.Enums;

using LoginRequest = LawPlatform.Entities.DTO.Account.Auth.Login.LoginRequest;
using ResetPasswordRequest = LawPlatform.Entities.DTO.Account.Auth.ResetPassword.ResetPasswordRequest;

namespace LawPlatform.DataAccess.Services.Auth
{
    public interface IAuthService
    {
        #region Login
        Task<Response<LoginResponse>> LoginAsync(LoginRequest model);
        #endregion

        #region Register Customer
        Task<Response<CustomerRegisterResponse>> RegisterCustomerAsync(CustomerRegisterRequest model);
        #endregion

        #region Register Lawyer
        Task<Response<LawyerRegisterResponse>> RegisterLawyerAsync(LawyerRegisterRequest model);
        #endregion

        #region Forgot / Reset Password
        Task<Response<ForgetPasswordResponse>> ForgotPasswordAsync(ForgetPasswordRequest model);
        Task<Response<ResetPasswordResponse>> ResetPasswordAsync(ResetPasswordRequest model);
        #endregion
        
        
        Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken);
        Task<Response<string>> LogoutAsync(ClaimsPrincipal userClaims);

        Task<Response<string>> ChangePasswordAsync(ClaimsPrincipal user, ChangePasswordRequest request);
       
    }
}
