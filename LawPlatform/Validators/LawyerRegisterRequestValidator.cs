using FluentValidation;
using LawPlatform.Entities.DTO.Account.Auth.Register;

namespace Ecommerce.API.Validators
{
    public class LawyerRegisterRequestValidator : AbstractValidator<LawyerRegisterRequest>
    {
        public LawyerRegisterRequestValidator() 
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be valid (e.g., user@example.com).");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^\+?\d{10,15}$").WithMessage("Phone number must contain only digits and be between 10 and 15 characters.");



            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.");

            RuleFor(x => x.BankAccountNumber)
                .NotEmpty().WithMessage("Bank account number is required.");

            RuleFor(x => x.BankName)
                .NotEmpty().WithMessage("Bank name is required.");

            RuleFor(x => x.Country)
                .NotEmpty().WithMessage("Country is required.");

            RuleFor(x => x.Bio)
                .NotEmpty().WithMessage("Bio is required.")
                .MaximumLength(500).WithMessage("Bio cannot exceed 500 characters.");

            RuleFor(x => x.Qualifications)
                .NotEmpty().WithMessage("Qualifications are required.");

            RuleFor(x => x.YearsOfExperience)
                .GreaterThanOrEqualTo(0).WithMessage("Years of experience must be 0 or greater.");

            RuleFor(x => x.QualificationDocument)
                .NotNull().WithMessage("Qualification document is required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
                .Matches(@"[!@#$%^&*]").WithMessage("Password must contain at least one special character (!@#$%^&*).");

        }
    }
}
