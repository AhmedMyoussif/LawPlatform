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
                return BadRequest(new Response<CreateConsultationResponse>
                {
                    Succeeded = false,
                    Message = "Invalid request data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList()
                });
            }

            var result = await _consultationService.CreateConsultationAsync(request);

            if (!result.Succeeded)
            {
                if (result.Message == "Validation failed")
                    return BadRequest(result);

                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }
    }
}
