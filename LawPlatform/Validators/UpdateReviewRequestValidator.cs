using FluentValidation;
using LawPlatform.Entities.DTO.Review;
namespace LawPlatform.API.Validators;

public class UpdateReviewRequestValidator : AbstractValidator<AddReviewRequest>
{
    public UpdateReviewRequestValidator()
    {
        RuleFor(x => x.Comment)
            .MaximumLength(1000).WithMessage("Comment must not exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Comment));

        RuleFor(x => x.Rating)
            .InclusiveBetween(1.0, 5.0).WithMessage("Rating must be between 1.0 and 5.0.");
    }
}
