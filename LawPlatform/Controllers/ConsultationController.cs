using System.Security.Claims;
using LawPlatform.DataAccess.Services.Consultation;
using LawPlatform.Entities.DTO.Consultation;
using LawPlatform.Entities.Shared.Bases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace LawPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsultationController : ControllerBase
    {
        private readonly IConsultationService _consultationService;
        private readonly ILogger<ConsultationController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public ConsultationController(IConsultationService consultationService, ILogger<ConsultationController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _consultationService = consultationService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("Client")]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> CreateConsultation([FromForm] CreateConsultationRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid request model for CreateConsultation");
                return BadRequest(new Response<GetConsultationResponse>
                {
                    Succeeded = false,
                    Message = "Invalid request data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList()
                });
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("nameid")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");
            var result = await _consultationService.CreateConsultationAsync(request);

            if (!result.Succeeded)
            {
                if (result.Message == "Validation failed")
                    return BadRequest(result);

                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpGet("consultations")]
        public async Task<IActionResult> GetAllConsultations([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _consultationService.GetAllConsultationsAsync(pageNumber, pageSize);
            return Ok(result);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetConsultationById(string id)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var result = await _consultationService.GetConsultationByIdAsync(id);
            if (!result.Succeeded)
            {
                if (result.Message == "Consultation not found")
                    return NotFound(result);
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }
            return Ok(result);
        }
        [HttpGet]
        public async Task<ActionResult<Response<List<GetConsultationResponse>>>> GetConsultations(
            [FromQuery] ConsultationFilterRequest filter , [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _consultationService.GetConsultationsAsync(filter, pageNumber ,pageSize);
            return Ok(result);
        }

        [HttpGet("MyLatestConsultations")]
        [Authorize(Roles = "Client , Lawyer")]
        public async Task<ActionResult<Response<List<GetConsultationResponse>>>> GetMyLatestConsultations()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("nameid")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");
            var result = await _consultationService.GetMyLatestConsultationsAsync();
            return Ok(result);
        }

        [HttpGet("MyConsultationsInProgress")]
        [Authorize (Roles = "Client , Lawyer")]
        public async Task<ActionResult<Response<List<GetConsultationResponse>>>> GetMyConsultationsInProgress()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("nameid")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");
            var result = await _consultationService.GetMyConsultationsInProgressAsync();
            return Ok(result);
        }

        [HttpPost("SearchLawyersByName")]
        public async Task<ActionResult<Response<List<LawyerSearchResponse>>>> SearchLawyersByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Name parameter is required.");
            }

            var result = await _consultationService.SearchLawyersByNameAsync(name);

            if (result == null || result.Data == null || !result.Data.Any())
                return NotFound("No lawyers found.");

            return Ok(result);
        }

        [HttpGet("MyConsultations")]
        public async Task<ActionResult<Response<List<GetConsultationResponse>>>> GetMyConsultations()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("nameid")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var result = await _consultationService.GetMyConsultationsAsync();

            return Ok(result);
        }


        [HttpGet("NewestOrders")]
        [Authorize(Roles = "Lawyer")]
        public async Task<ActionResult<Response<List<GetConsultationResponse>>>> GetNewestOrders()
        {
            return await _consultationService.GetNewestOrders();
        }

    }
}
