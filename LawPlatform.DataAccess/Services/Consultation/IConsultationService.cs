using System.Linq.Expressions;
using LawPlatform.Entities.DTO.Consultaion;
using LawPlatform.Entities.Shared.Bases;

namespace LawPlatform.DataAccess.Services.Consultation;

public interface IConsultationService
{
    Task<Response<GetConsultationResponse>> CreateConsultationAsync(CreateConsultationRequest request);
    Task<Response<PaginatedResult<GetConsultationResponse>>> GetAllConsultationsAsync(
     int pageNumber = 1, int pageSize = 10);
    Task<Response<GetConsultationResponse>> GetConsultationByIdAsync(string consultationId);

    //Task<Response<Guid>> DeleteConsultationAsync(string consultationId);

    Task<Response<List<ShowAllConsultaionWithoutDetails>>> GetConsultationsAsync(ConsultationFilterRequest filter);

    Task<Response<List<GetConsultationResponse>>> GetMyLatestConsultationsAsync();

    Task<Response<List<GetConsultationResponse>>> GetMyConsultationsInProgressAsync();
    Task<Response<List<ShowAllConsultaionWithoutDetails>>> GetMyConsultationsAsync();
    Task<Response<List<LawyerSearchResponse>>> SearchLawyersByNameAsync(string name);
}