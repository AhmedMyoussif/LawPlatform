using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LawPlatform.Entities.Models.Auth.Identity;
using LawPlatform.Entities.Models.Auth.UserTokens;
using LawPlatform.Utilities.Configurations;
using LawPlatform.DataAccess.ApplicationContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LawPlatform.DataAccess.Services.Token
{
    public class TokenStoreService : ITokenStoreService
    {
        private readonly SymmetricSecurityKey _symmetricSecurityKey;
        private readonly UserManager<User> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly LawPlatformContext _authContext;

        public TokenStoreService(IOptions<JwtSettings> jwtOptions, UserManager<User> userManager, LawPlatformContext authContext)
        {
            _jwtSettings = jwtOptions.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
            _userManager = userManager;

            if (string.IsNullOrEmpty(_jwtSettings.SigningKey))
                throw new ArgumentException("JWT SigningKey is not configured.");

            _symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SigningKey));
            _authContext = authContext;
        }

        public async Task<string> CreateAccessTokenAsync(User appUser)
        {
            var roles = await _userManager.GetRolesAsync(appUser);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, appUser.Id),
                new Claim(ClaimTypes.Email, appUser.Email ?? string.Empty),
                new Claim(ClaimTypes.GivenName, appUser.UserName ?? string.Empty)
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var creds = new SigningCredentials(_symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(30),
                SigningCredentials = creds,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }

        public async Task SaveRefreshTokenAsync(string userId, string refreshToken)
        {
            var userExists = await _authContext.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                throw new InvalidOperationException($"Cannot save refresh token: User with Id '{userId}' does not exist.");

            await _authContext.UserRefreshTokens.AddAsync(new UserRefreshToken
            {
                UserId = userId,
                Token = refreshToken,
                ExpiryDateUtc = DateTime.UtcNow.AddDays(7),
                IsUsed = false,
            });

            await _authContext.SaveChangesAsync();
        }

        public async Task InvalidateOldTokensAsync(string userId)
        {
            var tokens = await _authContext.UserRefreshTokens
                .Where(r => r.UserId == userId)
                .ToListAsync();

            if (tokens.Any())
            {
                _authContext.UserRefreshTokens.RemoveRange(tokens);
                await _authContext.SaveChangesAsync();
            }
        }

        public async Task<bool> IsValidAsync(string refreshToken)
        {
            return await _authContext.UserRefreshTokens
                .AnyAsync(r => r.Token == refreshToken && !r.IsUsed && r.ExpiryDateUtc > DateTime.UtcNow);
        }

        public async Task<(string AccessToken, string RefreshToken)> GenerateAndStoreTokensAsync(User user)
        {
            var accessToken = await CreateAccessTokenAsync(user);
            var refreshToken = GenerateRefreshToken();

            await SaveRefreshTokenAsync(user.Id, refreshToken);

            return (accessToken, refreshToken);
        }
    }
}
