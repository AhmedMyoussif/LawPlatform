using LawPlatform.Entities.DTO.Account.Auth.Register;
using LawPlatform.Entities.Shared.Bases;

using Google.Apis.Auth;

namespace LawPlatform.DataAccess.Services.OAuth
{
    public interface IAuthGoogleService
    {
        Task<Response<GoogleRegisterResponse>> AuthenticateWithGoogleAsync(string idToken);
        Task<GoogleJsonWebSignature.Payload> ValidateGoogleTokenAsync(string idToken);

    }
}
