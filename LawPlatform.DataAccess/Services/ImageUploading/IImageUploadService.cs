using Microsoft.AspNetCore.Http;
using LawPlatform.Entities.DTO.ImageUploading;

namespace LawPlatform.DataAccess.Services.ImageUploading
{
    public interface IImageUploadService
    {
        Task<UploadImageResponse> UploadAsync(IFormFile file);

        Task<bool> DeleteAsync(string publicId);
    }
}
