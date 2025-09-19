using FluentValidation;
using LawPlatform.Entities.DTO.Profile;
using Microsoft.AspNetCore.Http;
using System.IO;

public class UpdateClientProfileRequestValidator : AbstractValidator<UpdateClientProfileRequest>
{
    private readonly string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
    private const long maxFileSize = 5 * 1024 * 1024; // 5 MB

    public UpdateClientProfileRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(50).WithMessage("First name must be less than 50 characters.");

        RuleFor(x => x.LastName)
            .MaximumLength(50).WithMessage("Last name must be less than 50 characters.");

        RuleFor(x => x.Address)
            .MaximumLength(200).WithMessage("Address must be less than 200 characters.");

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
