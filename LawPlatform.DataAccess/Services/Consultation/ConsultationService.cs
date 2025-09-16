using System.Security.Claims;
using CloudinaryDotNet;
using FluentValidation;
using LawPlatform.DataAccess.ApplicationContext;
using LawPlatform.DataAccess.Services.ImageUploading;
using LawPlatform.Entities.DTO.Consultaion;
using LawPlatform.Entities.DTO.Proposal;
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
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? _httpContextAccessor.HttpContext?.User.FindFirst("nameid")?.Value;

            var client = await _context.Clients
           .FirstOrDefaultAsync(c => c.UserId == userId);
            if (client == null)
            {
                _logger.LogWarning("Client not found for UserId: {UserId}", userId);
                return _responseHandler.BadRequest<GetConsultationResponse>("Client does not exist.");
            }
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
             
                ClientId = client.Id,
                Title = request.Title,
                Description = request.Description,
                LawyerId = request.LawyerId,
                Specialization = request.Specialization,
                CreatedAt = DateTime.UtcNow,
                Budget = request.budget,
                Duration = request.duration,
                Status = ConsultationStatus.Active,
                Files = new List<ConsultationFile>()
               
            };

            // For Refactor : Make It in Helper Method 
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
                Budget = consultation.Budget,
                Duration = consultation.Duration,
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
            Status = c.Status,
            Budget = c.Budget,
            Duration = c.Duration,
            UrlFiles = c.Files.Select(f => f.FilePath).ToList(),
            LawyerId = c.LawyerId,
            Specialization = c.Specialization,
            ClientId = c.ClientId,
      
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

    public async Task<Response<GetConsultationResponse>> GetConsultationByIdAsync(string consultationId)
    {
        try
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? _httpContextAccessor.HttpContext?.User.FindFirst("nameid")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User not authenticated.");
                return _responseHandler.Unauthorized<GetConsultationResponse>("User not authenticated.");
            }

            _logger.LogInformation("Starting GetConsultationByIdAsync for ConsultationId: {ConsultationId}", consultationId);

            if (!Guid.TryParse(consultationId, out var consultationGuid))
            {
                _logger.LogWarning("Invalid ConsultationId format: {ConsultationId}", consultationId);
                return _responseHandler.BadRequest<GetConsultationResponse>("Invalid ConsultationId format.");
            }

            var consultation = await _context.consultations
                .Include(c => c.Files)
                .Include(c => c.Proposals)
                .ThenInclude(p => p.Lawyer)
                .FirstOrDefaultAsync(c => c.Id == consultationGuid);

            if (consultation == null)
            {
                _logger.LogWarning("Consultation not found: {ConsultationId}", consultationId);
                return _responseHandler.NotFound<GetConsultationResponse>("Consultation not found.");
            }

            bool isClientOwner = consultation.ClientId == userId;

            var visibleProposals = isClientOwner
                ? consultation.Proposals
                : consultation.Proposals.Where(p => p.Lawyer.UserId == userId).ToList();

            var consultationResponse = new GetConsultationResponse
            {
                Id = consultation.Id,
                Title = consultation.Title,
                Description = consultation.Description,
                CreatedAt = consultation.CreatedAt,
                ClientId = consultation.ClientId,
                LawyerId = consultation.LawyerId,
                Specialization = consultation.Specialization,
                Budget = consultation.Budget,
                Duration = consultation.Duration,
                Status = consultation.Status,
                UrlFiles = consultation.Files.Select(f => f.FilePath).ToList(),

                Proposals = visibleProposals.Select(p => new GetProposalResponse
                {
                    Id = p.Id,
                    Amount = p.Amount,
                    Description = p.Description,
                    DurationTime = p.DurationTime,
                    CreatedAt = p.CreatedAt,
                    Status = p.Status
                }).ToList()
            };

            _logger.LogInformation("Successfully retrieved consultation: {ConsultationId}", consultationId);
            return _responseHandler.Success(consultationResponse, "Consultation retrieved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving consultation: {ConsultationId}", consultationId);
            return _responseHandler.ServerError<GetConsultationResponse>("An error occurred while retrieving the consultation.");
        }
    }


    // For Filtering Consultations Based on Specialization, Budget Range, and Sorting by Newest or Oldest
    public async Task<Response<List<ShowAllConsultaionWithoutDetails>>> GetConsultationsAsync(ConsultationFilterRequest filter)
    {
        var query = _context.consultations.AsQueryable();

        if (filter.Specialization != default)
            query = query.Where(c => c.Specialization == filter.Specialization);

        if (filter.MinBudget.HasValue)
            query = query.Where(c => c.Budget >= filter.MinBudget.Value);

        if (filter.MaxBudget.HasValue)
            query = query.Where(c => c.Budget <= filter.MaxBudget.Value);

        if (!string.IsNullOrEmpty(filter.Sort))
        {
            query = filter.Sort.ToLower() == "newest"
                ? query.OrderByDescending(c => c.CreatedAt)
                : query.OrderBy(c => c.CreatedAt);
        }

        var consultationResponses = await query
            .Include(c => c.Files)
            .Select(c => new ShowAllConsultaionWithoutDetails
            {
                Id = c.Id,
                Title = c.Title,
                ClientId = c.ClientId,
              
            })
            .ToListAsync();

        return _responseHandler.Success(consultationResponses, "Consultations retrieved successfully");
    }

    // It is Not Allowed To Delete Consultation By Client 
    //public async Task<Response<Guid>> DeleteConsultationAsync(string consultationId)
    //{
    //    try
    //    {
    //        _logger.LogInformation("Starting DeleteConsultationAsync for ConsultationId: {ConsultationId}", consultationId);
    //        if (!Guid.TryParse(consultationId, out var consultationGuid))
    //        {
    //            _logger.LogWarning("Invalid ConsultationId format: {ConsultationId}", consultationId);
    //            return _responseHandler.BadRequest<Guid>("Invalid ConsultationId format.");
    //        }
    //        var consultation = await _context.consultations
    //            .Include(c => c.Files)
    //            .FirstOrDefaultAsync(c => c.Id == consultationGuid);
    //        if (consultation == null)
    //        {
    //            _logger.LogWarning("Consultation not found: {ConsultationId}", consultationId);
    //            return _responseHandler.NotFound<Guid>("Consultation not found.");
    //        }

    //        _context.consultations.Remove(consultation);
    //        await _context.SaveChangesAsync();
    //        _logger.LogInformation("Successfully deleted consultation: {ConsultationId}", consultationId);
    //        return _responseHandler.Success(consultation.Id, "Consultation deleted successfully.");
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error occurred while deleting consultation: {ConsultationId}", consultationId);
    //        return _responseHandler.ServerError<Guid>("An error occurred while deleting the consultation.");
    //    }
    //}

    // Insure That You Will Retrive The Consultations For The Current Logged In Client Only
    public async Task<Response<List<GetConsultationResponse>>> GetMyLatestConsultationsAsync()
    {
        try
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? _httpContextAccessor.HttpContext?.User.FindFirst("nameid")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User not authenticated.");
                return _responseHandler.Unauthorized<List<GetConsultationResponse>>("User not authenticated.");
            }
            var consultations = await _context.consultations
                .Where(c => c.Client.UserId == userId || c.LawyerId == userId)
                
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .ToListAsync();
            var consultationResponses = consultations.Select(c => new GetConsultationResponse
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                ClientId = c.ClientId,
                Budget = c.Budget,
                Duration = c.Duration,
                Status = c.Status,
                //UrlFiles = c.Files.Select(f => f.FilePath).ToList()
            }).ToList();
            return _responseHandler.Success(consultationResponses, "Latest consultations retrieved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving latest consultations.");
            return _responseHandler.ServerError<List<GetConsultationResponse>>("An error occurred while retrieving latest consultations.");
        }
    }
    // Insure That You Will Retrive The Consultations For The Current Logged In Client Only

    public async Task<Response<List<GetConsultationResponse>>> GetMyConsultationsInProgressAsync()
    {
        try
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? _httpContextAccessor.HttpContext?.User.FindFirst("nameid")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User not authenticated.");
                return _responseHandler.Unauthorized<List<GetConsultationResponse>>("User not authenticated.");
            }
           
            var consultations = await _context.consultations
                .Where(c => c.Client.UserId == userId && c.Status == ConsultationStatus.InProgress || c.LawyerId == userId && c.Status == ConsultationStatus.InProgress)
                .Include(c => c.Files)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            var consultationResponses = consultations.Select(c => new GetConsultationResponse
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                ClientId = c.ClientId,
                Budget = c.Budget,
                Duration = c.Duration,
                Status = c.Status,
                UrlFiles = c.Files.Select(f => f.FilePath).ToList()
            }).ToList();
            return _responseHandler.Success(consultationResponses, "In-progress consultations retrieved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving in-progress consultations.");
            return _responseHandler.ServerError<List<GetConsultationResponse>>("An error occurred while retrieving in-progress consultations.");
        }
    }


    public async Task<Response<List<LawyerSearchResponse>>> SearchLawyersByNameAsync(string name)
    {
        var lawyers = await _context.Lawyers
            .Where(l => l.Status == ApprovalStatus.Approved &&
                       (l.FirstName.Contains(name) || l.LastName.Contains(name)))
            .Select(l => new LawyerSearchResponse
            {
                Id = l.Id,
                FullName = l.FirstName + " " + l.LastName,
                Bio = l.Bio,
                Experiences = l.Experiences,
                Qualifications = l.Qualifications,
                YearsOfExperience = l.YearsOfExperience,
                Age = l.Age,
                Address = l.Address,
                Specialization = l.Specialization,
                Country = l.Country
            })
            .ToListAsync();

        return _responseHandler.Success(lawyers, "Lawyers fetched successfully");
    }



    public async Task<Response<List<ShowAllConsultaionWithoutDetails>>> GetMyConsultationsAsync()
    {
        try
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? _httpContextAccessor.HttpContext?.User.FindFirst("nameid")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User not authenticated.");
                return _responseHandler.Unauthorized<List<ShowAllConsultaionWithoutDetails>>("User not authenticated.");
            }
            var consultations = await _context.consultations
                 .Include(c => c.Files)
                 .Include(c => c.Proposals)
                 .Include(c => c.Client)
                 .Where(c => c.Client.UserId == userId || c.LawyerId == userId)
                 .OrderByDescending(c => c.CreatedAt)
                 .Take(5)
                 .ToListAsync();

            var consultationResponses = consultations.Select(c => new ShowAllConsultaionWithoutDetails
            {
                Id = c.Id,
                Title = c.Title,
                ClientId = c.ClientId,
               //UrlFiles = c.Files.Select(f => f.FilePath).ToList()
            }).ToList();
            return _responseHandler.Success(consultationResponses, "All consultations retrieved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all consultations.");
            return _responseHandler.ServerError<List<ShowAllConsultaionWithoutDetails>>("An error occurred while retrieving all consultations.");
        }
    }
 }