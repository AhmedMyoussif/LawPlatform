using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using LawPlatform.Utilities.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using LawPlatform.Entities.DTO.ImageUploading;
using Microsoft.Extensions.Logging;

namespace LawPlatform.DataAccess.Services.ImageUploading
{
    public class CloudinaryImageUploadService : IImageUploadService
    {
        private readonly Cloudinary _cloudinary;
        private readonly CloudinarySettings _cloudinarySettings;
        private readonly ILogger<CloudinaryImageUploadService> _logger;

        public CloudinaryImageUploadService(IOptions<CloudinarySettings> cloudinaryOptions, ILogger<CloudinaryImageUploadService> logger)
        {
            _cloudinarySettings = cloudinaryOptions.Value ?? throw new ArgumentNullException(nameof(cloudinaryOptions));
            var account = new Account(_cloudinarySettings.CloudName, _cloudinarySettings.ApiKey, _cloudinarySettings.ApiSecret);

            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
            _logger = logger;
        }

        public async Task<UploadImageResponse> UploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null", nameof(file));

            await using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            UploadResult? result;

            if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" ||
                extension == ".gif" || extension == ".bmp" || extension == ".webp")
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, memoryStream),
                    Folder = "consultations" 
                };

                result = await _cloudinary.UploadAsync(uploadParams);
            }
            else
            {
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, memoryStream),
                    Folder = "consultations" 
                };

                result = await _cloudinary.UploadAsync(uploadParams);
            }

            if (result == null)
                throw new Exception("Upload result was null from Cloudinary.");

            if (result.Error != null)
                throw new Exception($"Cloudinary error occurred: {result.Error.Message}");

            var url = result.SecureUrl?.ToString() ?? result.Url?.ToString();
            if (string.IsNullOrWhiteSpace(url))
                throw new Exception("Cloudinary returned no URL for uploaded resource.");

            _logger.LogInformation("Cloudinary upload succeeded. PublicId={PublicId} Url={Url}", result.PublicId, url);

            return new UploadImageResponse
            {
                Url = url,
                PublicId = result.PublicId
            };
        }


        public async Task<bool> DeleteAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                return false;

            var deletionParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deletionParams);

            return result.Result == "ok";
        }
    }
}
