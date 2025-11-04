using LawPlatform.Entities.DTO.Profile;
using LawPlatform.Entities.Shared.Bases;
using Microsoft.AspNetCore.Http;

namespace LawPlatform.DataAccess.Services.Profile
{
    public interface IProfileService
    {
        Task<Response<object>> GetProfileAsync();
        Task<Response<bool>> UpdateClientProfileAsync(UpdateClientProfileRequest dto);
        Task<Response<bool>> UpdateLawyerProfileAsync(UpdateLawyerProfileRequest dto);
        Task<Response<bool>> UpdateProfileImageAsync(IFormFile profileImage);
    }
}
