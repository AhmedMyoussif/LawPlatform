using System.Security.Claims;
using LawPlatform.DataAccess.Services.Profile;
using LawPlatform.Entities.DTO.Profile;
using LawPlatform.Entities.Shared.Bases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet]
        public async Task<ActionResult<Response<ClientProfileResponse>>> GetProfile()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var result = await _ProfileService.GetProfileAsync(userId);
            return StatusCode((int)result.StatusCode, result);
        }
        
        [HttpPut]
        public async Task<ActionResult<Response<bool>>> UpdateProfile([FromBody] UpdateClientProfileRequest dto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            if (userId == null)
                return Unauthorized();

            var result = await _ProfileService.UpdateProfileAsync(userId, dto);
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
