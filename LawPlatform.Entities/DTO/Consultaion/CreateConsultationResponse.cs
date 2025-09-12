using LawPlatform.Entities.Models;
using LawPlatform.Utilities.Enums;

namespace LawPlatform.Entities.DTO.Consultaion;

public class CreateConsultationResponse
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string ClientId { get; set; }
    public decimal budget { get; set; }
        
    public int duration { get; set; }
    public ConsultationStatus Status { get; set; }
    public string CategoryId { get; set; }
    public string? UrlFiles { get; set; }
}