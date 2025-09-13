using FluentValidation;
using LawPlatform.Entities.DTO.Account.Auth.Register;

namespace LawPlatform.API.Validators
{
    public class LawyerRegisterRequestValidator : AbstractValidator<LawyerRegisterRequest>
    {
        public LawyerRegisterRequestValidator()
        {
            // Email
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be valid (e.g., user@example.com).");

            // Phone
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^\+?\d{10,15}$").WithMessage("Phone number must be between 10 and 15 digits (optionally starting with +).");

            // FirstName & LastName
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

            // UserName
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username is required.")
                .MaximumLength(100).WithMessage("Username cannot exceed 100 characters.");

            // Password
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
                .Matches(@"[!@#$%^&*]").WithMessage("Password must contain at least one special character (!@#$%^&*).");

            // Bio
            RuleFor(x => x.Bio)
                .NotEmpty().WithMessage("Bio is required.")
                .MaximumLength(500).WithMessage("Bio cannot exceed 500 characters.");

            // Qualifications
            RuleFor(x => x.Qualifications)
                .NotEmpty().WithMessage("Qualifications are required.");


            RuleFor(x => x.Experiences)
                .MaximumLength(500).WithMessage("Experiences cannot exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Experiences));

            // Years of experience
            RuleFor(x => x.YearsOfExperience)
                .GreaterThanOrEqualTo(0).WithMessage("Years of experience must be 0 or greater.");

            // License
            RuleFor(x => x.LicenseNumber)
                .NotEmpty().WithMessage("License number is required.");

            RuleFor(x => x.LicenseDocument)
                .NotNull().WithMessage("License document is required.");

 
            // Country
            RuleFor(x => x.Country)
                .NotEmpty().WithMessage("Country is required.");

            // Bank details
            RuleFor(x => x.IBAN)
                .NotEmpty().WithMessage("IBAN is required.")
                .Matches(@"^[A-Z0-9]{15,34}$").WithMessage("IBAN must be between 15 and 34 alphanumeric characters.");

            RuleFor(x => x.BankName)
                .NotEmpty().WithMessage("Bank name is required.")
                .MaximumLength(150).WithMessage("Bank name cannot exceed 150 characters.");

            RuleFor(x => x.BankAccountNumber)
                .MaximumLength(50).WithMessage("Bank account number cannot exceed 50 characters.")
                .When(x => !string.IsNullOrEmpty(x.BankAccountNumber));

            // Documents
            RuleFor(x => x.QualificationDocument)
                .NotNull().WithMessage("Qualification document is required.");
        }
    }
}
