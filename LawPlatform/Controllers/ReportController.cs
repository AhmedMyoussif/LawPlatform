using LawPlatform.DataAccess.Services.Report;
using LawPlatform.Entities.DTO.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LawPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService rportService)
        {
            _reportService = rportService;
        }

        /// <summary>
        /// Creates a new report for a specific consultation.
        /// </summary>
        /// <remarks>
        /// This endpoint allows authenticated users (Clients or Lawyers)  
        /// to submit a report about a consultation.  
        /// <br/>
        /// Example request:
        /// <br/>
        /// POST /api/Reports
        /// {
        ///     "consultationId": "guid",
        ///     "reason": "Unprofessional behavior",
        ///     "description": "The lawyer didn't attend the session."
        /// }
        /// </remarks>
        [HttpPost]
        public async Task<IActionResult> AddReport([FromBody] AddReportRequest request)
        {
            var response = await _reportService.AddReportAsync(request);
            if (!response.Succeeded)
            {
                return StatusCode((int)response.StatusCode, response);
            }
            return Ok(response);
        }
        /// <summary>
        /// Retrieves a specific report by its unique ID (Admin only).
        /// </summary>
        /// <remarks>
        /// This endpoint returns detailed information about a single report,  
        /// including the consultation, reporter, and reported lawyer details.  
        /// </remarks>

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetReportById(Guid id)
        {
            var response = await _reportService.GetReportByIdAsync(id);
            if (!response.Succeeded)
                return StatusCode((int)response.StatusCode, response);

            return Ok(response);
        }
        /// <summary>
        /// Retrieves all reports in the system (Admin only).
        /// </summary>
        /// <remarks>
        /// This endpoint returns a list of all submitted reports,  
        /// including the reporter, reported lawyer, and consultation details.  
        /// Results are ordered by creation date (most recent first).
        /// </remarks>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllReports()
        {
            var response = await _reportService.GetAllReportsAsync();
            if (!response.Succeeded)
                return StatusCode((int)response.StatusCode, response);

            return Ok(response);
        }

    }
}
