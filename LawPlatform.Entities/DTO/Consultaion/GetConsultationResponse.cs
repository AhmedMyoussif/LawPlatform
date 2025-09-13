using LawPlatform.Entities.Models;
using LawPlatform.Utilities.Enums;

namespace LawPlatform.Entities.DTO.Consultaion;

public class GetConsultationResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string ClientId { get; set; }
    public decimal budget { get; set; }
    public string? LawyerId { get; set; }
    public int duration { get; set; }
    public ConsultationStatus Status { get; set; }
    public Specialization Specialization { get; set; }
    public List<string> UrlFiles { get; set; }
}