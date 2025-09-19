using FluentValidation;
using LawPlatform.Entities.DTO.Consultaion;
using Microsoft.AspNetCore.Http;
using System.Linq;

public class CreateConsultationRequestValidator : AbstractValidator<CreateConsultationRequest>
{
    private readonly string[] allowedExtensions = { ".pdf", ".doc", ".docx" };

    public CreateConsultationRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must be less than 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.");

        RuleFor(x => x.budget)
            .GreaterThan(0).WithMessage("Budget must be greater than 0.");

        RuleFor(x => x.duration)
            .GreaterThan(0).WithMessage("Duration must be greater than 0.");

        RuleFor(x => x.Specialization)
            .IsInEnum().WithMessage("Invalid specialization.");

        RuleFor(x => x.Files)
            .Must(files => files == null || files.Count <= 10)
            .WithMessage("You can upload a maximum of 10 files.");

        RuleForEach(x => x.Files)
            .Must(file => IsValidFileExtension(file))
            .WithMessage("Only PDF or Word files are allowed.");
    }

    private bool IsValidFileExtension(IFormFile file)
    {
        var extension = System.IO.Path.GetExtension(file.FileName).ToLower();
        return allowedExtensions.Contains(extension);
    }
}
