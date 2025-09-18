using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Entities.DTO.Profile;
using LawPlatform.Entities.Shared.Bases;

namespace LawPlatform.DataAccess.Services.Profile
{
    public interface IProfileService
    {
        Task<Response<object>> GetProfileAsync();
        Task<Response<bool>>UpdateProfileAsync(UpdateClientProfileRequest dto);
       // Task<Response<bool>>UpdateProfilePictureAsync(string userId, UpdateClientProfileRequest dto);
    }
}
