using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using LawPlatform.DataAccess.ApplicationContext;
using LawPlatform.DataAccess.Services.Consultation;
using LawPlatform.DataAccess.Services.ImageUploading;
using LawPlatform.Entities.DTO.Consultaion;
using LawPlatform.Entities.DTO.Proposal;
using LawPlatform.Entities.Models.Auth.Users;
using LawPlatform.Entities.Shared.Bases;
using LawPlatform.Utilities.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LawPlatform.DataAccess.Services.Proposal
{
    public class ProposalService: IProposalService
    {
        private readonly LawPlatformContext _context;
        private readonly ResponseHandler _responseHandler;
        private readonly ILogger<ConsultationService> _logger;
        private readonly IValidator<CreateConsultationRequest> _validator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ProposalService(IHttpContextAccessor httpContextAccessor, IValidator<CreateConsultationRequest> validator, ILogger<ConsultationService> logger, ResponseHandler responseHandler, LawPlatformContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _validator = validator;
            _logger = logger;
            _responseHandler = responseHandler;
            _context = context;
        }
        public async Task<Response<GetProposalResponse>> SubmitProposalAsync(SubmitPropsalRequest dto)
        {
            _logger.LogInformation("Submitting proposal...");
            var userId = _httpContextAccessor.HttpContext.User
            .Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;

            var lawyer = await _context.Lawyers.FirstOrDefaultAsync(l => l.UserId == userId);
            if (lawyer == null)
                return _responseHandler.BadRequest<GetProposalResponse>("Only lawyers can submit proposals.");

            try
            {
                var proposal = new Entities.Models.Proposal
                {
                    Amount = dto.Amount,
                    CreatedAt = DateTime.UtcNow,
                    Description = dto.Description,
                    DurationTime = dto.DurationTime,
                    LawyerId = lawyer.Id,
                    ConsultationId = dto.ConsultationId,
                    Status = ProposalStatus.Pending
                };

                _context.Proposals.Add(proposal);
                await _context.SaveChangesAsync();

                var result = new GetProposalResponse
                {
                    Id = proposal.Id,
                    Amount = proposal.Amount,
                    Description = proposal.Description,
                    Status = proposal.Status,
                    DurationTime = proposal.DurationTime,
                    LawyerId = proposal.LawyerId,
                    CreatedAt = proposal.CreatedAt
                };

                return _responseHandler.Success(result, "Proposal submitted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while submitting proposal");
                return _responseHandler.BadRequest<GetProposalResponse>("An error occurred while submitting proposal");
            }
        }

    }
}
