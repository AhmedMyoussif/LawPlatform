namespace LawPlatform.Entities.DTO.Profile
{
    public class LawyerProfileResponse
    {
        public string Id { get; set; }
        public string Role { get; set; }

        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }

        public int Age { get; set; }
        public string Address { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string Country { get; set; }
        public string Bio { get; set; }
        public string Experiences { get; set; }

        public string Qualifications { get; set; }

        public int YearsOfExperience { get; set; }

        public string LicenseNumber { get; set; }

        public string Specialization { get; set; }

        public string BankName { get; set; }

        public string LicenseDocument { get; set; }
        public string QualificationDocument { get; set; }
    }
}




