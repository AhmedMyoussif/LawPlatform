using LawPlatform.DataAccess.Services.Consultation;
using LawPlatform.DataAccess.Services.Proposal;
using LawPlatform.Entities.DTO.Proposal;
using LawPlatform.Entities.Shared.Bases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LawPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProposalController : ControllerBase
    {
        private readonly ResponseHandler _responseHandler;
        private readonly ILogger<ConsultationService> _logger;
        private readonly IProposalService _ProposalService;

        public ProposalController(ResponseHandler responseHandler, ILogger<ConsultationService> logger, IProposalService proposalService)
        {
            _responseHandler = responseHandler;
            _logger = logger;
            _ProposalService = proposalService;
        }

        [HttpPost]
        public async Task<ActionResult<GetProposalResponse>> SubmitPropsalAsync(SubmitPropsalRequest dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid request model for Submit Proposal");

                var errors = ModelState.Values
                   .SelectMany(v => v.Errors)
                   .Select(e => e.ErrorMessage)
                   .ToList();
                var errorMessage = string.Join(" | ", errors);
                return BadRequest(_responseHandler.BadRequest<string>(errorMessage));
            }
            var result = await _ProposalService.SubmitProposalAsync(dto);

            return Ok(result);
        }

        [HttpGet("{consultationId}")]
        public async Task<ActionResult<List<GetProposalResponse>>> GetProposalsByConsultationIdAsync(Guid consultationId)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt to GetProposalsByConsultationIdAsync");
                return Unauthorized(_responseHandler.Unauthorized<string>("User is not authenticated."));
            }
            if (consultationId == Guid.Empty)
            {
                _logger.LogWarning("Invalid consultationId for GetProposalsByConsultationIdAsync");
                return BadRequest(_responseHandler.BadRequest<string>("Invalid consultationId."));
            }
            var result = await _ProposalService.GetProposalsByConsultationIdAsync(consultationId);
            return Ok(result);
        }
        [HttpGet("{proposalId}")]
        public async Task<ActionResult<GetProposalResponse>> GetProposalByIdAsync(Guid proposalId)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt to GetProposalByIdAsync");
                return Unauthorized(_responseHandler.Unauthorized<string>("User is not authenticated."));
            }
            if (proposalId == Guid.Empty)
            {
                _logger.LogWarning("Invalid proposalId for GetProposalByIdAsync");
                return BadRequest(_responseHandler.BadRequest<string>("Invalid proposalId."));
            }
            var result = await _ProposalService.GetProposalByIdAsync(proposalId);
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("accept/{proposalId}")]
        [Authorize(Roles = "Client")]

        public async Task<ActionResult<AcceptProposalResponse>> AcceptProposalAsync(Guid proposalId)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt to AcceptProposalAsync");
                return Unauthorized(_responseHandler.Unauthorized<string>("User is not authenticated."));
            }
            if (proposalId == Guid.Empty)
            {
                _logger.LogWarning("Invalid proposalId for AcceptProposalAsync");
                return BadRequest(_responseHandler.BadRequest<string>("Invalid proposalId."));
            }
            var result = await _ProposalService.AcceptProposalAsync(proposalId);
            if (!result.Succeeded)
                return BadRequest(result);
            return Ok(result);
        }
    }
}
