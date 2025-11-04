using FluentValidation;
using LawPlatform.Entities.DTO.Profile;
using Microsoft.AspNetCore.Http;

namespace LawPlatform.API.Validators;

public class UpdateLawyerProfileRequestValidator : AbstractValidator<UpdateLawyerProfileRequest>
{
    private readonly string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
    private const long maxFileSize = 5 * 1024 * 1024; // 5 MB

    public UpdateLawyerProfileRequestValidator()
    {
        // FirstName - optional but validate if provided
        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        // LastName - optional but validate if provided
        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.LastName));

        // UserName - optional but validate if provided
        RuleFor(x => x.UserName)
            .MaximumLength(100).WithMessage("Username cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.UserName));

        // Bio - optional but validate if provided
        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("Bio cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Bio));

        // Experiences - optional but validate if provided
        RuleFor(x => x.Experiences)
            .MaximumLength(500).WithMessage("Experiences cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Experiences));

        // Qualifications - optional but validate if provided
        RuleFor(x => x.Qualifications)
            .MaximumLength(500).WithMessage("Qualifications cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Qualifications));

        // YearsOfExperience - optional but validate if provided
        RuleFor(x => x.YearsOfExperience)
            .GreaterThanOrEqualTo(0).WithMessage("Years of experience must be 0 or greater.")
            .When(x => x.YearsOfExperience.HasValue);

        // Age - optional but validate if provided
        RuleFor(x => x.Age)
            .InclusiveBetween(18, 100).WithMessage("Age must be between 18 and 100.")
            .When(x => x.Age.HasValue);

        // Address - optional but validate if provided
        RuleFor(x => x.Address)
            .MaximumLength(200).WithMessage("Address cannot exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.Address));

        // Country - optional but validate if provided
        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Country));

        // IBAN - optional but validate if provided
        RuleFor(x => x.IBAN)
            .Matches(@"^[A-Z0-9]{15,34}$").WithMessage("IBAN must be between 15 and 34 alphanumeric characters.")
            .When(x => !string.IsNullOrEmpty(x.IBAN));

        // BankAccountNumber - optional but validate if provided
        RuleFor(x => x.BankAccountNumber)
            .MaximumLength(50).WithMessage("Bank account number cannot exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.BankAccountNumber));

        // BankName - optional but validate if provided
        RuleFor(x => x.BankName)
            .MaximumLength(150).WithMessage("Bank name cannot exceed 150 characters.")
            .When(x => !string.IsNullOrEmpty(x.BankName));

        // ProfileImage - optional but validate if provided
        RuleFor(x => x.ProfileImage)
            .Must(file => file == null || IsValidFile(file))
            .WithMessage("Profile image must be JPG or PNG and less than 5MB.");
    }

    private bool IsValidFile(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLower();
        return allowedExtensions.Contains(extension) && file.Length <= maxFileSize;
    }
}
