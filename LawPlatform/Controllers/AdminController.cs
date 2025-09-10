using LawPlatform.DataAccess.Services.Admin;
using LawPlatform.DataAccess.Services.Auth;
using LawPlatform.Entities.DTO.Account.Auth.Admin;
using LawPlatform.Entities.Shared.Bases;
using LawPlatform.Utilities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LawPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IAdminService _adminService;
        public AdminController(IAuthService authService, IAdminService adminService)
        {
            _authService = authService;
            _adminService = adminService;
        }

        #region Get Lawyers by Status
        [HttpGet("lawyers")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Response<List<GetLawyerByStatusResponse>>>> GetLawyersByStatus([FromQuery] ApprovalStatus? status)
        {
            var result = await _adminService.GetLawyersByStatusAsync(status);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }
        #endregion

        #region Update Lawyer Account Status
        [HttpPut("lawyers/{lawyerId}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Response<UpdateLawyerAccountStatusResponse>>> UpdateLawyerAccountStatus(
            string lawyerId,
            [FromBody] UpdateLawyerAccountStatusRequest model)
        {
            if (lawyerId != model.LawyerId)
                return BadRequest("LawyerId in URL does not match request body.");

            var result = await _adminService.UpdateLawyerAccountStatusAsync(model);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }
        #endregion
    }
}
