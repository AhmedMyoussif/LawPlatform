using LawPlatform.Entities.DTO.Consultaion;
using LawPlatform.Entities.Shared.Bases;

namespace LawPlatform.DataAccess.Services.Consultation;

public interface IConsultationService
{
    Task<Response<GetConsultationResponse>> CreateConsultationAsync(CreateConsultationRequest request,string clientid);
    Task<Response<PaginatedResult<GetConsultationResponse>>> GetAllConsultationsAsync(
     int pageNumber = 1, int pageSize = 10);
    Task<Response<GetConsultationResponse>> GetConsultationByIdAsync(string consultationId);

    Task<Response<Guid>> DeleteConsultationAsync(string consultationId);

    Task<Response<List<GetConsultationResponse>>> GetConsultationsAsync(ConsultationFilterRequest filter);
}