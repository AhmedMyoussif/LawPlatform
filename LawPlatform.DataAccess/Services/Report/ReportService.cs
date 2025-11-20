using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.DataAccess.ApplicationContext;
using LawPlatform.Entities.DTO.Report;
using LawPlatform.Entities.Shared.Bases;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LawPlatform.DataAccess.Services.Report
{
    public class ReportService : IReportService
    {
        private readonly ResponseHandler _responseHandler;
        private readonly ILogger<ReportService> _logger;
        private readonly LawPlatformContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReportService(ResponseHandler responseHandler, ILogger<ReportService> logger, LawPlatformContext context, IHttpContextAccessor httpContextAccessor)
        {
            _responseHandler = responseHandler;
            _logger = logger;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Response<GetReportResponse>> AddReportAsync(AddReportRequest request)
        {

            var ReporterId = _httpContextAccessor.HttpContext?.User?
              .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (ReporterId == null)
            {
                _logger.LogWarning("Unauthorized attempt to add report for ConsultationId: {ConsultationId}", request.ConsultationId);
                return _responseHandler.Unauthorized<GetReportResponse>("User is not authenticated.");
            }

            var consultation = await _context.consultations
            .Include(c => c.Lawyer)
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == request.ConsultationId);

            if (consultation == null)
            {
                _logger.LogWarning("Consultation with Id: {ConsultationId} not found", request.ConsultationId);
                return _responseHandler.NotFound<GetReportResponse>("Consultation not found.");
            }
            if (consultation.ClientId != ReporterId && ReporterId != consultation.LawyerId)
            {
                _logger.LogWarning("Unauthorized report attempt by user {ReporterId} on consultation {ConsultationId}",
                    ReporterId, request.ConsultationId);
                return _responseHandler.Unauthorized<GetReportResponse>("You are not authorized to report this consultation.");
            }
            var report = new Entities.Models.Report
            {
                Reason = request.Reason,
                Description = request.Description,
                ConsultationId = request.ConsultationId,
                ReporterId = ReporterId,
                ReportedLawyerId = consultation.LawyerId
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            var response = new GetReportResponse
            {
                Id = report.Id,
                Reason = report.Reason,
                Description = report.Description,
                ConsultationId = report.ConsultationId,
                ReporterId = report.ReporterId,
                ReportedUserId = report.ReportedLawyerId,
                CreatedAt = report.CreatedAt
            };
            _logger.LogInformation("Report with Id: {ReportId} successfully created for ConsultationId: {ConsultationId}", report.Id, request.ConsultationId);
            return _responseHandler.Created(response);
        }

        public async Task<Response<GetReportResponse>> GetReportByIdAsync(Guid id)
        {
            _logger.LogInformation("Admin requested report with Id: {ReportId}", id);

            var report = await _context.Reports
                .Include(r => r.Consultation)
                .Include(r => r.Reporter)
                .Include(r => r.ReportedLawyer)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
            {
                _logger.LogWarning("Report with Id: {ReportId} not found", id);
                return _responseHandler.NotFound<GetReportResponse>("Report not found.");
            }

            var response = new GetReportResponse
            {
                Id = report.Id,
                Reason = report.Reason,
                Description = report.Description,
                ConsultationId = report.ConsultationId,
                ReporterId = report.ReporterId,
                ReportedUserId = report.ReportedLawyerId,
                CreatedAt = report.CreatedAt, 
            };

            _logger.LogInformation("Successfully retrieved report with Id: {ReportId}", id);
            return _responseHandler.Success<GetReportResponse>(response , "Report Retrived Successfully");
        }

        public async Task<Response<IEnumerable<GetReportResponse>>> GetAllReportsAsync()
        {
            _logger.LogInformation("Admin requested all reports");

            var reports = await _context.Reports
                .Include(r => r.Consultation)
                .Include(r => r.Reporter)
                .Include(r => r.ReportedLawyer)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            if (!reports.Any())
            {
                _logger.LogWarning("No reports found");
                return _responseHandler.NotFound<IEnumerable<GetReportResponse>>("No reports found.");
            }

            var response = reports.Select(report => new GetReportResponse
            {
                Id = report.Id,
                Reason = report.Reason,
                Description = report.Description,
                ConsultationId = report.ConsultationId,
                ReporterId = report.ReporterId,
                ReportedUserId = report.ReportedLawyerId,
                CreatedAt = report.CreatedAt,
            });

            _logger.LogInformation("Successfully retrieved {Count} reports", reports.Count);
            return _responseHandler.Success<IEnumerable<GetReportResponse>>(response, "Reports Retrived Successfully");
        }


    }
}
