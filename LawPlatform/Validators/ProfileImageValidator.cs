using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace LawPlatform.API.Validators
{
    public class ProfileImageFileValidator : AbstractValidator<IFormFile>
    {
        private const int MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };

        private bool HasValidExtension(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            return AllowedExtensions.Contains(extension);
        }

        public ProfileImageFileValidator()
        {
            RuleFor(x => x)
                .NotNull().WithMessage("Image file is required.")
                .Must(x => x.Length > 0).WithMessage("Image file cannot be empty.")
                .Must(x => x.Length <= MAX_FILE_SIZE).WithMessage("Image file size must be less than 5MB.")
                .Must(HasValidExtension).WithMessage("Only .jpg, .jpeg, and .png files are allowed.");
        }
    }
}
