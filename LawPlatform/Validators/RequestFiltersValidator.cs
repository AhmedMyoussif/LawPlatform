using LawPlatform.Entities.DTO.Shared;
using FluentValidation;

namespace LawPlatform.API.Validators;

public class RequestFiltersValidator<TSorting> : AbstractValidator<RequestFilters<TSorting>>
    where TSorting : struct, Enum
{
    public RequestFiltersValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100.");
    }
}
