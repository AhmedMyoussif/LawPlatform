using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using LawPlatform.Utilities.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using LawPlatform.Entities.DTO.ImageUploading;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace LawPlatform.DataAccess.Services.ImageUploading
{
    public class UploadcareService : IFileUploadService
    {
        private readonly HttpClient _httpClient;
        private readonly UploadcareSettings _settings;
        private readonly ILogger<UploadcareService> _logger;

        public UploadcareService(
            IOptions<UploadcareSettings> uploadcareOptions,
            ILogger<UploadcareService> logger,
            HttpClient httpClient)
        {
            _settings = uploadcareOptions.Value ?? throw new ArgumentNullException(nameof(uploadcareOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
                throw new InvalidOperationException("Uploadcare API Key is not configured.");
            
            if (string.IsNullOrWhiteSpace(_settings.CdnUrl))
                throw new InvalidOperationException("Uploadcare CDN URL is not configured.");
        }

        public async Task<UploadFileResponse> UploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null", nameof(file));

            try
            {
                await using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                // Create multipart form data
                using var content = new MultipartFormDataContent();

                // Add the public key
                content.Add(new StringContent(_settings.ApiKey), "UPLOADCARE_PUB_KEY");

                // Add the file
                var fileContent = new StreamContent(memoryStream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                    file.ContentType ?? "application/octet-stream");
                content.Add(fileContent, "file", file.FileName);

                // Optional: Add metadata
                content.Add(new StringContent("true"), "UPLOADCARE_STORE");

                // Upload to Uploadcare
                var uploadUrl = _settings.UploadUrl;
                var response = await _httpClient.PostAsync(uploadUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Uploadcare upload failed. Status: {StatusCode}, Error: {Error}",
                        response.StatusCode, errorContent);
                    throw new Exception($"Uploadcare upload failed with status {response.StatusCode}: {errorContent}");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var uploadResult = JsonSerializer.Deserialize<UploadcareResponse>(responseJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (uploadResult == null || string.IsNullOrWhiteSpace(uploadResult.File))
                {
                    _logger.LogError("Uploadcare returned invalid response: {Response}", responseJson);
                    throw new Exception("Uploadcare returned an invalid response.");
                }

                // Construct the CDN URL
                var cdnUrl = $"{_settings.CdnUrl.TrimEnd('/')}/{uploadResult.File}/";

                _logger.LogInformation("Uploadcare upload succeeded. FileId={FileId} Url={Url}",
                    uploadResult.File, cdnUrl);

                return new UploadFileResponse
                {
                    Url = cdnUrl
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error occurred during Uploadcare upload");
                throw new Exception("Network error occurred while uploading to Uploadcare.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during Uploadcare upload for file: {FileName}", file.FileName);
                throw;
            }
        }


        // Internal class for deserializing Uploadcare upload response
        
    }
}
