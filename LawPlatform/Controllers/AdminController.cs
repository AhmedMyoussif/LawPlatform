using Google.Apis.Services;
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
    [Authorize(Roles ="Admin")]

    public class AdminController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;
        public AdminController(IAuthService authService, IAdminService adminService, ILogger<AdminController> logger)
        {
            _authService = authService;
            _adminService = adminService;
            _logger = logger;
        }

        #region Get Lawyers by Status
        [HttpGet("lawyers")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Response<List<GetLawyerResponse>>>> GetLawyersByStatus([FromQuery] ApprovalStatus? status)
        {
            var result = await _adminService.GetLawyersByStatusAsync(status);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }
        #endregion

        #region Update Lawyer Account Status
        [HttpPut("lawyers/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Response<UpdateLawyerAccountStatusResponse>>> UpdateLawyerAccountStatus(
           
            [FromForm] UpdateLawyerAccountStatusRequest model)
        {
          
            var result = await _adminService.UpdateLawyerAccountStatusAsync(model);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }
        #endregion

        #region Get Lawyer by Id
        [HttpGet("lawyers/{lawyerId}")]
         public async Task<ActionResult<Response<GetLawyerResponse>>> GetLawyerById(string lawyerId)
        {
            var result = await _adminService.GetLawyerByIdAsync(lawyerId);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }
        #endregion

        #region Get All Clients
        [HttpGet("all/clients")]
        public async Task<IActionResult> GetAllClients()
        {
            _logger.LogInformation("HTTP GET /api/clients/all called");

            var response = await _adminService.GetAllClients();

            if (!response.Succeeded)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("client{id}")]
        public async Task<IActionResult> GetClientById(string id)
        {
            _logger.LogInformation("HTTP GET /api/clients/{Id} called", id);

            var response = await _adminService.GetClientById(id);

            if (!response.Succeeded)
                return BadRequest(response);

            return Ok(response);
        }
        #endregion

        #region Delete Account  

        [HttpDelete("delete/{userId}")]
        //[Authorize("Admin")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount(Guid userId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("HTTP DELETE /api/admin/delete/{UserId} called", userId);
            var response = await _adminService.DeleteAccountAsync(userId, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }
        #endregion
    }
}
