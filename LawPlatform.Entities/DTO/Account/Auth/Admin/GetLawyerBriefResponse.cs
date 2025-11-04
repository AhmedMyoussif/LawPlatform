using LawPlatform.Utilities.Enums;

namespace LawPlatform.Entities.DTO.Account.Auth.Admin;
public class GetLawyerBriefResponse
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public ApprovalStatus Status { get; set; }
    public double? Rating { get; set; }
    public int YearsOfExperience { get; set; }
    public string Specialization { get; set; }
    public string? Experiences { get; set; }
    public string ProfileImageUrl { get; set; }
    public int CompletedConsultations { get; set; }
}