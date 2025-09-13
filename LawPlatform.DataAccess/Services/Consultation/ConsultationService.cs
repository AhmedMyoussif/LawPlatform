using System.Security.Claims;
using CloudinaryDotNet;
using FluentValidation;
using LawPlatform.DataAccess.ApplicationContext;
using LawPlatform.DataAccess.Services.ImageUploading;
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

            var consultation = new Entities.Models.Consultation
            {
             
                ClientId = clientid,
                Title = request.Title,
                Description = request.Description,
                LawyerId = request.LawyerId,
                Specialization = request.Specialization,
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
                Specialization = consultation.Specialization,
                LawyerId = consultation.LawyerId,
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
    public async Task<Response<PaginatedResult<GetConsultationResponse>>> GetAllConsultationsAsync(
     int pageNumber = 1, int pageSize = 10)
    {
        _logger.LogInformation("Retrieving consultations - Page {Page}, Size {Size}", pageNumber, pageSize);

        if (pageNumber <= 0 || pageSize <= 0)
            return _responseHandler.BadRequest<PaginatedResult<GetConsultationResponse>>("Invalid pagination parameters.");

        var query = _context.consultations
            .Include(c => c.Files)
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync();

        if (totalCount == 0)
        {
            _logger.LogWarning("No consultations found");
            return _responseHandler.NotFound<PaginatedResult<GetConsultationResponse>>("No consultations found.");
        }

        var consultations = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

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
            UrlFiles = c.Files.Select(f => f.FilePath).ToList()
        }).ToList();

        var result = new PaginatedResult<GetConsultationResponse>
        {
            Items = consultationResponses,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        _logger.LogInformation("Retrieved {Count} consultations on page {Page}", consultationResponses.Count, pageNumber);

        return _responseHandler.Success(result, "Consultations retrieved successfully.");
    }

}