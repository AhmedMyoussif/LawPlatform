using Microsoft.AspNetCore.Http;
using LawPlatform.Entities.DTO.ImageUploading;

namespace LawPlatform.DataAccess.Services.ImageUploading
{
    public interface IFileUploadService
    {
        Task<UploadFileResponse> UploadAsync(IFormFile file);
    }
}
