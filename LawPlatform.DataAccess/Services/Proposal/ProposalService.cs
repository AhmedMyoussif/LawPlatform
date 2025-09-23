using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
    public class ProposalService : IProposalService
    {
        private readonly LawPlatformContext _context;
        private readonly ResponseHandler _responseHandler;
        private readonly ILogger<ProposalService> _logger;
        private readonly IValidator<CreateConsultationRequest> _validator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ProposalService(IHttpContextAccessor httpContextAccessor, IValidator<CreateConsultationRequest> validator, ILogger<ProposalService> logger, ResponseHandler responseHandler, LawPlatformContext context)
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
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? _httpContextAccessor.HttpContext?.User.FindFirst("nameid")?.Value;

            var lawyer = await _context.Lawyers.FirstOrDefaultAsync(l => l.Id == userId);
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
                    CreatedAt = proposal.CreatedAt,
                    //lawyer = new GetLawyerResponse
                };

                return _responseHandler.Success(result, "Proposal submitted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while submitting proposal");
                return _responseHandler.BadRequest<GetProposalResponse>("An error occurred while submitting proposal");
            }
        }

        public async Task<Response<List<GetProposalResponse>>> GetProposalsByConsultationIdAsync(Guid consultationId)
        {
            _logger.LogInformation("Fetching proposals for consultation ID: {ConsultationId}", consultationId);
            try
            {
                var proposals = await _context.Proposals
                    .Where(p => p.ConsultationId == consultationId)
                    .Select(p => new GetProposalResponse
                    {
                        Id = p.Id,
                        Amount = p.Amount,
                        Description = p.Description,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        DurationTime = p.DurationTime,
                        //LawyerId = p.LawyerId,
                        //ConsultationId = p.ConsultationId,
                        Status = p.Status
                    })
                    .ToListAsync();
                if (proposals == null || !proposals.Any())
                {
                    _logger.LogWarning("No proposals found for consultation ID: {ConsultationId}", consultationId);
                    return _responseHandler.NotFound<List<GetProposalResponse>>("No proposals found for the given consultation ID.");
                }
                return _responseHandler.Success(proposals, "Proposals fetched successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching proposals for consultation ID: {ConsultationId}", consultationId);
                return _responseHandler.BadRequest<List<GetProposalResponse>>("An error occurred while fetching proposals");
            }
        }

        public async Task<Response<GetProposalResponse>> GetProposalByIdAsync(Guid proposalId)
        {
            _logger.LogInformation("Fetching proposal by ID: {ProposalId}", proposalId);

            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? _httpContextAccessor.HttpContext?.User.FindFirst("nameid")?.Value;

            if (string.IsNullOrEmpty(userId))
                return _responseHandler.Unauthorized<GetProposalResponse>("User not logged in.");

            try
            {
                var lawyer = await _context.Lawyers.FirstOrDefaultAsync(l => l.Id == userId);
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == userId);

                IQueryable<LawPlatform.Entities.Models.Proposal> query = _context.Proposals;

                if (lawyer != null)
                {
                    query = query.Where(p => p.Id == proposalId && p.LawyerId == lawyer.Id);
                }
                else if (client != null)
                {
                    query = query.Where(p => p.Id == proposalId && p.Consultation.ClientId == client.Id);
                }
                else
                {
                    return _responseHandler.BadRequest<GetProposalResponse>("Only lawyers or clients can access proposals.");
                }

                var proposal = await query
                    .Select(p => new GetProposalResponse
                    {
                        Id = p.Id,
                        Amount = p.Amount,
                        Description = p.Description,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        DurationTime = p.DurationTime,
                        //LawyerId = p.LawyerId,
                        //ConsultationId = p.ConsultationId,
                        Status = p.Status
                    })
                    .FirstOrDefaultAsync();

                if (proposal == null)
                {
                    _logger.LogWarning("Proposal not found or not accessible by user: {ProposalId}", proposalId);
                    return _responseHandler.NotFound<GetProposalResponse>("Proposal not found or not accessible.");
                }

                return _responseHandler.Success(proposal, "Proposal fetched successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching proposal by ID: {ProposalId}", proposalId);
                return _responseHandler.BadRequest<GetProposalResponse>("An error occurred while fetching the proposal");
            }
        }

        public async Task<Response<AcceptProposalResponse>> AcceptProposalAsync(Guid proposalId)
        {
            _logger.LogInformation("Accepting proposal ID: {ProposalId}", proposalId);
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? _httpContextAccessor.HttpContext?.User.FindFirst("nameid")?.Value;
            if (string.IsNullOrEmpty(userId))
                return _responseHandler.Unauthorized<AcceptProposalResponse>("User not logged in.");
            try
            {
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == userId);
                if (client == null)
                    return _responseHandler.BadRequest<AcceptProposalResponse>("Only clients can accept proposals.");
                var proposal = await _context.Proposals
                    .Include(p => p.Consultation)
                    .FirstOrDefaultAsync(p => p.Id == proposalId && p.Consultation.ClientId == client.Id);
                if (proposal == null)
                {
                    _logger.LogWarning("Proposal not found or not accessible by client: {ProposalId}", proposalId);
                    return _responseHandler.NotFound<AcceptProposalResponse>("Proposal not found or not accessible.");
                }
               
                proposal.Status = ProposalStatus.Accepted;
                proposal.UpdatedAt = DateTime.UtcNow;
                proposal.Consultation.Status = ConsultationStatus.InProgress;
                proposal.Consultation.UpdatedAt = DateTime.UtcNow;
                //var otherProposals = await _context.Proposals
                //    .Where(p => p.ConsultationId == proposal.ConsultationId && p.Id != proposalId && p.Status == ProposalStatus.Pending)
                //    .ToListAsync();
                //foreach (var otherProposal in otherProposals)
                //{
                //    otherProposal.Status = ProposalStatus.Rejected;
                //    otherProposal.UpdatedAt = DateTime.UtcNow;
                //}
                await _context.SaveChangesAsync();
                var result = new AcceptProposalResponse
                {
                    Status = proposal.Status
                };
                return _responseHandler.Success(result, "Proposal accepted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while accepting proposal ID: {ProposalId}", proposalId);
                return _responseHandler.BadRequest<AcceptProposalResponse>("An error occurred while accepting the proposal");
            }
        }

        public async Task<Response<GetProposalResponse>> GetMyProposalsAsync()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? _httpContextAccessor.HttpContext?.User.FindFirst("nameid")?.Value;
            if (string.IsNullOrEmpty(userId))
                return _responseHandler.Unauthorized<GetProposalResponse>("User not logged in.");

            try
            {
               var lawyer = await _context.Lawyers.FirstOrDefaultAsync(l=>l.Id == userId);
                if (lawyer == null) return _responseHandler.BadRequest<GetProposalResponse>("Lawyer Not Found.");
              
                var proposal = await _context.Proposals
                    .Include(p=>p.Consultation)
                    .FirstOrDefaultAsync(p=>p.LawyerId == userId && p.Consultation.LawyerId == lawyer.Id );

                if (proposal == null)
                {
                    _logger.LogError("You didn't submit any propsal.");
                    return _responseHandler.NotFound<GetProposalResponse>("You didn't submit any proposal yet.");
                }
                var result = new GetProposalResponse
                {
                    Id = proposal.Id,
                    Amount = proposal.Amount,
                    Description = proposal.Description,
                    CreatedAt = proposal.CreatedAt,
                    UpdatedAt = proposal.UpdatedAt,
                    DurationTime = proposal.DurationTime,
                    //LawyerId = p.LawyerId,
                    //ConsultationId = p.ConsultationId,
                    Status = proposal.Status
                };

                return _responseHandler.Success(result, "Proposal fetched successfully");
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex, "Error while fetching my proposals");
                return _responseHandler.BadRequest<GetProposalResponse>("An error occurred while fetching your proposals");
            }
        }
    }

}
