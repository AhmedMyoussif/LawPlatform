using LawPlatform.Utilities.Enums;

namespace LawPlatform.Entities.DTO.Account.Auth.Admin;
public class GetLawyerBriefResponse
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; } 
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string UserName { get; set; }

    public ApprovalStatus Status { get; set; }
    public double? Rating { get; set; }
    public int Age { get; set; }
    public string Address { get; set; } 

    public string Bio { get; set; } 

    public int YearsOfExperience { get; set; }
    public string Specialization { get; set; }
    public string Qualifications { get; set; }
    public int YersOfExperience { get; set; } 
    public string LicenseNumber { get; set; }  

    public string LicenseDocument { get; set; }

    public string BankName { get; set; }
    public string Experiences { get; set; }
    public string ProfileImageUrl { get; set; }
    public int CompletedConsultations { get; set; }
    public string CreatedAt { get; set; }
}