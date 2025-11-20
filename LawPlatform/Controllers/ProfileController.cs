using LawPlatform.DataAccess.Services.Profile;
using LawPlatform.Entities.DTO.Profile;
using LawPlatform.Entities.Shared.Bases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LawPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]

    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _ProfileService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProfileController(IProfileService ProfileService, IHttpContextAccessor httpContextAccessor)
        {
            _ProfileService = ProfileService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Get the authenticated user's profile (Client or Lawyer)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<Response<ClientProfileResponse>>> GetProfile()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var result = await _ProfileService.GetProfileAsync();
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Update client profile
        /// </summary>
        [HttpPut("client")]
        [Authorize(Roles = "Client")]
        public async Task<ActionResult<Response<bool>>> UpdateProfile([FromForm] UpdateClientProfileRequest dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();



            var result = await _ProfileService.UpdateClientProfileAsync(dto);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Update lawyer profile
        /// </summary>
        [HttpPut("lawyer")]
        [Authorize(Roles = "Lawyer")]
        public async Task<ActionResult<Response<bool>>> UpdateLawyerProfile([FromForm] UpdateLawyerProfileRequest dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var result = await _ProfileService.UpdateLawyerProfileAsync(dto);
            return StatusCode((int)result.StatusCode, result);
        }


        //[HttpPut("image")]
        //public async Task<ActionResult<Response<string>>> UpdateProfileImage([FromForm] IFormFile newImage)
        //{
        //    var userId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        //    if (userId == null)
        //        return Unauthorized();

        //    var result = await _clientProfileService.UpdateProfileImageAsync(userId, newImage);
        //    return StatusCode(result.StatusCode, result);
        //}
    }
}
