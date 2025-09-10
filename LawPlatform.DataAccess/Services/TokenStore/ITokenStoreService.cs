using LawPlatform.Entities.Models.Auth.Identity;

namespace LawPlatform.DataAccess.Services.Token
{
    public interface ITokenStoreService
    {
        Task<string> CreateAccessTokenAsync(User appUser);
        string GenerateRefreshToken();
        Task SaveRefreshTokenAsync(string userId, string refreshToken);
        Task InvalidateOldTokensAsync(string userId);
        Task<bool> IsValidAsync(string refreshToken);


    }
}
