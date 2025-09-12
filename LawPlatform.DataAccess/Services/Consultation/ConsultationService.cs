using System.Security.Claims;
using CloudinaryDotNet;
using FluentValidation;
using LawPlatform.DataAccess.ApplicationContext;
using LawPlatform.DataAccess.Services.ImageUploading;
using LawPlatform.Entities.DTO.Category;
using LawPlatform.Entities.DTO.Consultaion;
using LawPlatform.Entities.Models;
using LawPlatform.Entities.Models.Auth.Identity;
using LawPlatform.Entities.Shared.Bases;
using LawPlatform.Utilities.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LawPlatform.DataAccess.Services.Consultation;

public class ConsultationService :  IConsultationService
{
    private readonly LawPlatformContext _context;
    private readonly ResponseHandler _responseHandler;
    private readonly ILogger<ConsultationService> _logger;
    private readonly IValidator<CreateConsultationRequest> _validator;
    private readonly IImageUploadService _imageUploadService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ConsultationService(LawPlatformContext context, ResponseHandler responseHandler, ILogger<ConsultationService> logger, IImageUploadService imageUploadService, IValidator<CreateConsultationRequest> validator, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _responseHandler = responseHandler;
        _logger = logger;
        _imageUploadService = imageUploadService;
        _validator = validator;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Response<GetConsultationResponse>> CreateConsultationAsync(CreateConsultationRequest request,string clientid)
    {
        try
        {
            _logger.LogInformation("Starting CreateConsultationAsync for Client");

            
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for consultation creation. Errors: {Errors}",
                    string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

                return _responseHandler.ValidationError<GetConsultationResponse>(
                    validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                );
            }
            //var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            //if (string.IsNullOrEmpty(clientId))
            //{
            //    _logger.LogWarning("ClientId could not be determined from the logged-in user.");
            //    return _responseHandler.Unauthorized<GetConsultationResponse>("User not authenticated.");
            //}
            //if (userId == null)
            //    userId = _httpContextAccessor.HttpContext?.User?.FindFirst("nameid")?.Value;
            //if (string.IsNullOrEmpty(userId))
            //{
            //    return _responseHandler.Unauthorized<GetConsultationResponse>("User not authenticated.");
            //}

            var consultation = new Entities.Models.Consultation
            {
                ClientId = clientid,
                Title = request.Title,
                Description = request.Description,
                CategoryId = request.CategoryId,
                CreatedAt = DateTime.UtcNow,
                budget = request.budget,
                duration = request.duration,
                Status = ConsultationStatus.Active,
                Files = new List<ConsultationFile>()
               
            };
            _context.Add(consultation);
            _context.SaveChanges();

            
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
                        FilePath = uploadResult.Url 
                    });

                    uploadedFiles.Add(uploadResult.Url);
                }
            }


            await _context.consultations.AddAsync(consultation);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Consultation {ConsultationId} created successfully for ClientId: {ClientId}",
                consultation.Id, consultation.ClientId);

            var consultationResponse = new GetConsultationResponse
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
            return _responseHandler.ServerError<GetConsultationResponse>("An error occurred while creating consultation");
        }
    }
    public async Task<Response<List<GetConsultationResponse>>> GetAllConsultationsAsync()
    {
        _logger.LogInformation("Retrieving all consultations");
        var consultations = _context.consultations.Include(c => c.Files).ToList();
        if (!consultations.Any())
        {
            _logger.LogWarning("No consultations found");
            return _responseHandler.NotFound<List<GetConsultationResponse>>("No consultations found.");
        }
        var consultationResponses = consultations.Select(c => new GetConsultationResponse
        {
            Id = c.Id,
            Title = c.Title,
            Description = c.Description,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            ClientId = c.ClientId,
            budget = c.budget,
            duration = c.duration,
            Status = c.Status,
            CategoryId = c.CategoryId,
            UrlFiles = c.Files.Select(f => f.FilePath).ToList()
        }).ToList();
        _logger.LogInformation("Retrieved {Count} consultations", consultationResponses.Count);
        return _responseHandler.Success(consultationResponses, "Consultations retrieved successfully.");

    }
}