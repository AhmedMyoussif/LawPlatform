using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

using LawPlatform.Utilities.Configurations;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using LawPlatform.Entities.DTO.ImageUploading;

namespace LawPlatform.DataAccess.Services.ImageUploading
{
    public class CloudinaryImageUploadService : IImageUploadService
    {
        private readonly Cloudinary _cloudinary;
        private readonly CloudinarySettings _cloudinarySettings;
        public CloudinaryImageUploadService(IOptions<CloudinarySettings> Cloudinaryoptions)
        {
            _cloudinarySettings = Cloudinaryoptions.Value ?? throw new ArgumentNullException(nameof(Cloudinaryoptions));
            var account = new Account(_cloudinarySettings.CloudName, _cloudinarySettings.ApiKey, _cloudinarySettings.ApiSecret);

            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }
        public async Task<UploadImageResponse> UploadAsync(IFormFile file)
        {
            // NOTE : Image validation must be done e.g(file extension....)
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null");

            await using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, memoryStream)
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result == null)
                throw new Exception("Upload result was null from Cloudinary.");

            if (result.Error != null)
                throw new Exception($"Cloudinary error occurred: {result.Error.Message}");

            return new UploadImageResponse{
                Url = result.SecureUrl?.ToString()!,
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
