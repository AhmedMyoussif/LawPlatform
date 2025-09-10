using LawPlatform.Entities.DTO.Account.Auth.ResetPassword;

using FluentValidation;
using LawPlatform.Entities.DTO.Account.Auth.ResetPassword;

namespace Ecommerce.API.Validators
{
    public class ForgetPasswordRequestValidator : AbstractValidator<ForgetPasswordRequest>
    {
        public ForgetPasswordRequestValidator()
        {
            RuleFor(x => x)
                .Must(x => !string.IsNullOrEmpty(x.Email) || !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Either email or phone number is required.");

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("Email must be valid (e.g., user@example.com).");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?\d{10,15}$").When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Phone number must contain only digits and be between 10 and 15 characters.");

        }
    }
}
