using CloudinaryDotNet;
using FluentValidation;
using LawPlatform.DataAccess.ApplicationContext;
using LawPlatform.DataAccess.Services.ImageUploading;
using LawPlatform.Entities.DTO.Category;
using LawPlatform.Entities.DTO.Consultaion;
using LawPlatform.Entities.Models;
using LawPlatform.Entities.Shared.Bases;
using LawPlatform.Utilities.Enums;
using Microsoft.Extensions.Logging;

namespace LawPlatform.DataAccess.Services.Consultation;

public class ConsultationService :  IConsultationService
{
    private readonly LawPlatformContext _context;
    private readonly ResponseHandler _responseHandler;
    private readonly ILogger<ConsultationService> _logger;
    private readonly IValidator<CreateConsultationRequest> _validator;
    private readonly IImageUploadService _imageUploadService;
    public ConsultationService(LawPlatformContext context , ResponseHandler responseHandler, ILogger<ConsultationService> logger, ICloudinary cloudinary , IImageUploadService imageUploadService)
    {
        _context = context;
        _responseHandler = responseHandler;
        _logger = logger;
        _imageUploadService  = imageUploadService;
    }

     public async Task<Response<CreateConsultationResponse>> CreateConsultationAsync(CreateConsultationRequest request)
    {
        try
        {
            _logger.LogInformation("Starting CreateConsultationAsync for Client");

            
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for consultation creation. Errors: {Errors}",
                    string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

                return _responseHandler.ValidationError<CreateConsultationResponse>(
                    validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                );
            }
            
       
            var consultation = new Entities.Models.Consultation
            {
                
                Title = request.Title,
                Description = request.Description,
                CategoryId = request.CategoryId,
                CreatedAt = DateTime.UtcNow,
                budget = request.budget,
                duration = request.duration,
                Status = ConsultationStatus.Active,
                Files = new List<ConsultationFile>()
               
            };
            
            var uploadedFiles = new List<string>();
            if (request.Files != null && request.Files.Count > 0)
            {
                foreach (var file in request.Files)
                {
                    var uploadResult = await _imageUploadService.UploadAsync(file);

                    consultation.Files.Add(new ConsultationFile
                    {
                        ConsultationId = consultation.Id,
                        FileName = file.FileName,
                        FilePath = uploadResult.Url // ✅ URL اللي رجع من Cloudinary
                    });

                    uploadedFiles.Add(uploadResult.Url);
                }
            }


            await _context.consultations.AddAsync(consultation);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Consultation {ConsultationId} created successfully for ClientId: {ClientId}",
                consultation.Id, consultation.ClientId);

            var consultationResponse = new CreateConsultationResponse
            {
                Id = consultation.Id,
                Title = consultation.Title,
                Description = consultation.Description,
                Status = consultation.Status,
                ClientId =  consultation.ClientId,
                CategoryId = consultation.CategoryId,
                budget = consultation.budget,
                duration = consultation.duration,
                UrlFiles = uploadedFiles
            };

            return _responseHandler.Success(consultationResponse, "Consultation created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating consultation for ClientI");
            return _responseHandler.ServerError<CreateConsultationResponse>("An error occurred while creating consultation");
        }
    }
}