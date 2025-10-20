using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using LawPlatform.Entities.DTO.Profile;
using LawPlatform.Entities.DTO.Proposal;
using LawPlatform.Entities.Models;
using LawPlatform.Entities.Models.Auth.Users;
using LawPlatform.Utilities.Enums;

namespace LawPlatform.Entities.DTO.Consultation;

public class GetConsultationResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ClientId { get; set; }

    public decimal Budget { get; set; }
    public string? LawyerId { get; set; }
    public int Duration { get; set; }
    public ConsultationStatus Status { get; set; }
    public Specialization Specialization { get; set; }
    public List<string> UrlFiles { get; set; }
    public ClientInfo Client { get; set; }
    public int ProposalsCount { get; set; }

    public List<GetProposalResponse> Proposals { get; set; }
    [JsonInclude]
    public string Slug
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Title))
                return "";

            var slug = Regex.Replace(Title.Trim(), @"[^\p{L}\p{N}\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-");
            slug = Regex.Replace(slug, "-{2,}", "-");
            return slug;
        }
    }
}