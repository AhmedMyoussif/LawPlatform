using System.Security.Claims;
using LawPlatform.DataAccess.ApplicationContext;
using LawPlatform.DataAccess.Services.Consultation;
using LawPlatform.DataAccess.Services.Proposal;
using LawPlatform.Entities.DTO.Proposal;
using LawPlatform.Entities.Shared.Bases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LawPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProposalController : ControllerBase
    {
        private readonly ResponseHandler _responseHandler;
        private readonly ILogger<ProposalService> _logger;
        private readonly IProposalService _ProposalService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LawPlatformContext _context;

        public ProposalController(ResponseHandler responseHandler, ILogger<ProposalService> logger, IProposalService proposalService, IHttpContextAccessor httpContextAccessor, LawPlatformContext context)
        {
            _responseHandler = responseHandler;
            _logger = logger;
            _ProposalService = proposalService;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
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

        [HttpGet("by-consultation/{consultationId}")]
        public async Task<ActionResult<List<GetProposalResponse>>> GetProposalsByConsultationIdAsync(Guid consultationId)

        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? _httpContextAccessor.HttpContext?.User.FindFirst("nameid")?.Value;
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
        
        [HttpGet("by-id/{id}")]
        public async Task<ActionResult<GetProposalResponse>> GetProposalByIdAsync(Guid id)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? _httpContextAccessor.HttpContext?.User.FindFirst("nameid")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt to GetProposalByIdAsync");
                return Unauthorized(_responseHandler.Unauthorized<string>("User is not authenticated."));
            }
            if (id == Guid.Empty)
            {
                _logger.LogWarning("Invalid proposalId for GetProposalByIdAsync");
                return BadRequest(_responseHandler.BadRequest<string>("Invalid proposalId."));
            }
            var result = await _ProposalService.GetProposalByIdAsync(id);
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("accept/{proposalId}")]
        [Authorize(Roles = "Client")]

        public async Task<ActionResult<AcceptProposalResponse>> AcceptProposalAsync(Guid proposalId)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? _httpContextAccessor.HttpContext?.User.FindFirst("nameid")?.Value;
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

        [HttpGet("HasProposal")]
        public async Task<Response<bool>> HasProposalAsync([FromQuery] string consultationId, [FromQuery] string lawyerId)
        {
            try
            {
                if (!Guid.TryParse(consultationId, out var consultationGuid))
                    return _responseHandler.BadRequest<bool>("Invalid ConsultationId format.");

                if (string.IsNullOrEmpty(lawyerId))
                    return _responseHandler.BadRequest<bool>("LawyerId is required.");

                var hasProposal = await _context.Proposals
                    .AnyAsync(p => p.ConsultationId == consultationGuid && p.LawyerId == lawyerId);

                return _responseHandler.Success(hasProposal, hasProposal
                    ? "Lawyer has already submitted a proposal for this consultation."
                    : "Lawyer has not submitted any proposal for this consultation.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking proposal for ConsultationId: {ConsultationId} and LawyerId: {LawyerId}", consultationId, lawyerId);
                return _responseHandler.ServerError<bool>("An error occurred while checking the proposal.");
            }
        }

    }
}
