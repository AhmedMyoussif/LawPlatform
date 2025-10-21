using LawPlatform.DataAccess.Services.Admin;
using LawPlatform.DataAccess.Services.Auth;
using LawPlatform.Entities.DTO.Account.Auth.Admin;
using LawPlatform.Entities.Shared.Bases;
using LawPlatform.Utilities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LawPlatform.Entities.DTO.Shared;
using FluentValidation;

namespace LawPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AdminController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;
        private readonly IValidator<RequestFilters<LawyerSorting>> _requestFiltersValidator;
        public AdminController(IAuthService authService, IAdminService adminService, ILogger<AdminController> logger, IValidator<RequestFilters<LawyerSorting>> requestFiltersValidator)
        {
            _authService = authService;
            _adminService = adminService;
            _logger = logger;
            _requestFiltersValidator = requestFiltersValidator;
        }


        [HttpGet("lawyers")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Response<List<GetLawyerBriefResponse>>>> GetLawyersByStatus([FromQuery] ApprovalStatus? status, [FromQuery] RequestFilters<LawyerSorting> filters)
        {
            var validationResult = await _requestFiltersValidator.ValidateAsync(filters);
            if (!validationResult.IsValid)
            {
                string errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return BadRequest(errors);
            }
            var result = await _adminService.GetLawyersByStatusAsync(status, filters);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("lawyers/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Response<UpdateLawyerAccountStatusResponse>>> UpdateLawyerAccountStatus(

            [FromBody] UpdateLawyerAccountStatusRequest model)
        {

            var result = await _adminService.UpdateLawyerAccountStatusAsync(model);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("lawyers/{lawyerId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Response<GetLawyerResponse>>> GetLawyerById(string lawyerId)
        {
            var result = await _adminService.GetLawyerByIdAsync(lawyerId);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("all/clients")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllClients(string?search)
        {
            _logger.LogInformation("HTTP GET /api/clients/all called");

            var response = await _adminService.GetAllClients(search);

            if (!response.Succeeded)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("client{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetClientById(string id)
        {
            _logger.LogInformation("HTTP GET /api/clients/{Id} called", id);

            var response = await _adminService.GetClientById(id);

            if (!response.Succeeded)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("MentorConsultations")]
        public async Task<IActionResult> GetMentorConsultations(string consultation , int pageNumber = 1 , int pageSize = 10 )
        {
            var response = await _adminService.MentorConsultationsync(consultation , pageNumber , pageSize);
            if (!response.Succeeded) return BadRequest(response);
            return Ok(response);
        }

        #region Delete Account  

        [HttpDelete("delete/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAccount(Guid userId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("HTTP DELETE /api/admin/delete/{UserId} called", userId);
            var response = await _adminService.DeleteAccountAsync(userId, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }
        #endregion


        [HttpGet("GetTotalConsultationsCount")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTotalConsultationsCount()
        {
            var response = await _adminService.GetTotalConsultationsCountAsync();
            if (!response.Succeeded) return BadRequest(response);
            return Ok(response);
        }
        [HttpGet("GetTotalClientsCount")]
        public async Task<IActionResult> GetTotalClientsCount()
        {
            var response = await _adminService.GetTotalClientsCountAsync();
            if (!response.Succeeded) return BadRequest(response);
            return Ok(response);
        }
        [HttpGet("GetApprovedLawyers")]
        [Authorize(Roles = "Admin,Client")]
        public async Task<IActionResult> GetApprovedLawyers([FromQuery] RequestFilters<LawyerSorting> filters)
        {
            var validationResult = await _requestFiltersValidator.ValidateAsync(filters);
            if (!validationResult.IsValid)
            {
                string errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return BadRequest(errors);
            }

            var response = await _adminService.GetLawyersByStatusAsync(ApprovalStatus.Approved, filters);

            if (!response.Succeeded) return BadRequest(response);
            return Ok(response);
        }

    }
}
