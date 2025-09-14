using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Entities.DTO.Profile;
using LawPlatform.Entities.Shared.Bases;

namespace LawPlatform.DataAccess.Services.Profile
{
    public interface IClientProfileService
    {
        Task<Response<ClientProfileResponse>> GetProfileAsync(string userId);
        Task<Response<bool>>UpdateProfileAsync(string userId, UpdateClientProfileRequest dto);
       // Task<Response<bool>>UpdateProfilePictureAsync(string userId, UpdateClientProfileRequest dto);
    }
}
