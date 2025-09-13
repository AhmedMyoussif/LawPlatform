using System.Security.Claims;
using LawPlatform.DataAccess.Services.Consultation;
using LawPlatform.Entities.DTO.Consultaion;
using LawPlatform.Entities.Shared.Bases;
using Microsoft.AspNetCore.Mvc;

namespace LawPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsultationController : ControllerBase
    {
        private readonly IConsultationService _consultationService;
        private readonly ILogger<ConsultationController> _logger;

        public ConsultationController(IConsultationService consultationService, ILogger<ConsultationController> logger)
        {
            _consultationService = consultationService;
            _logger = logger;
        }
        
        [HttpPost("Client")]
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
            var result = await _consultationService.CreateConsultationAsync(request, userId);

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

    }
}
