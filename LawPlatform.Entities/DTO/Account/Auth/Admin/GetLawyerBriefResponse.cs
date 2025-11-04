using LawPlatform.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public string? QualificationDocumentUrl { get; set; }
}