using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Entities.DTO.Report;
using LawPlatform.Entities.Shared.Bases;

namespace LawPlatform.DataAccess.Services.Report
{
    public interface IReportService
    {
        Task<Response<GetReportResponse>> AddReportAsync(AddReportRequest request);
        Task<Response<GetReportResponse>> GetReportByIdAsync(Guid id);
        Task<Response<IEnumerable<GetReportResponse>>> GetAllReportsAsync();
    }
}
