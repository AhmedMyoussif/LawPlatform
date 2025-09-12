using LawPlatform.Entities.DTO.Account.Auth.Admin;
using LawPlatform.Utilities.Enums;

using FluentValidation;
using LawPlatform.Entities.DTO.Account.Auth.Admin;
using LawPlatform.Utilities.Enums;

namespace LawPlatform.API.Validators
{
    public class GetLawyerRequestValidator : AbstractValidator<GetLawyerByStatusRequest>
    {
        public GetLawyerRequestValidator()
        {
            RuleFor(x => x.Status)
            .Must(x => !x.HasValue || Enum.IsDefined(typeof(ApprovalStatus), x.Value))
            .WithMessage("Invalid status value.");
        }
    }
}
