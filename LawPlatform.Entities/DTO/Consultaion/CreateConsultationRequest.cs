using LawPlatform.Entities.Models;
using LawPlatform.Utilities.Enums;
using Microsoft.AspNetCore.Http;

namespace LawPlatform.Entities.DTO.Consultation;

public class CreateConsultationRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal budget { get; set; }
    public string? LawyerId { get; set; }
    public int duration { get; set; }
   public Specialization Specialization { get; set; }
    public List<IFormFile>? Files { get; set; }

}