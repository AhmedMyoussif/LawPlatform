using LawPlatform.Entities.DTO.Category;
using LawPlatform.Entities.DTO.Consultaion;
using LawPlatform.Entities.Shared.Bases;

namespace LawPlatform.DataAccess.Services.Consultation;

public interface IConsultationService
{
    Task<Response<CreateConsultationResponse>> CreateConsultationAsync(CreateConsultationRequest request);
}