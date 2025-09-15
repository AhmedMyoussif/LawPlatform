using LawPlatform.DataAccess.Services.Consultation;
using LawPlatform.DataAccess.Services.Proposal;
using LawPlatform.Entities.DTO.Proposal;
using LawPlatform.Entities.Shared.Bases;
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

    }
}
