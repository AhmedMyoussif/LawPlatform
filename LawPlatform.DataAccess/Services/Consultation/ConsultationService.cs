using System.Security.Claims;
using CloudinaryDotNet;
using FluentValidation;
using LawPlatform.DataAccess.ApplicationContext;
using LawPlatform.DataAccess.Services.ImageUploading;
using LawPlatform.Entities.DTO.Consultaion;
using LawPlatform.Entities.DTO.ImageUploading;
using LawPlatform.Entities.DTO.Proposal;
using LawPlatform.Entities.Models;
using LawPlatform.Entities.Models.Auth.Identity;
using LawPlatform.Entities.Shared.Bases;
using LawPlatform.Utilities.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

    // for Client Only
    public async Task<Response<GetConsultationResponse>> CreateConsultationAsync(CreateConsultationRequest request)
    {
        try
        {
            var userId = ConsultationServiceHelper.GetCurrentUserId(_httpContextAccessor);

            var client = await _context.Clients
           .FirstOrDefaultAsync(c => c.Id == userId);
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

            await _context.consultations.AddAsync(consultation);
            await _context.SaveChangesAsync();

            var (consultationFiles, uploadedFiles) =
                 await UploadFilesAsync(request.Files, consultation.Id, _imageUploadService);

            foreach (var file in consultationFiles)
            {
                consultation.Files.Add(file);
            }
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
    public async Task<Response<PaginatedResult<GetConsultationResponse>>> GetAllConsultationsAsync(  // with out sort we can remove it 
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
            var userId = ConsultationServiceHelper.GetCurrentUserId(_httpContextAccessor);

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
                : consultation.Proposals.Where(p => p.Lawyer.Id == userId).ToList();

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
    public async Task<Response<PaginatedResult<GetConsultationResponse>>> GetConsultationsAsync(
    ConsultationFilterRequest filter, int pageNumber = 1, int pageSize = 10)
    {
        _logger.LogInformation("Retrieving consultations - Page {Page}, Size {Size}", pageNumber, pageSize);

        if (pageNumber <= 0 || pageSize <= 0)
            return _responseHandler.BadRequest<PaginatedResult<GetConsultationResponse>>("Invalid pagination parameters.");

        var query = _context.consultations
            .Include(c=>c.Client)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(c =>
             EF.Functions.Contains(c.Title, $"\"{filter.SearchTerm}\"") ||
             EF.Functions.Contains(c.Description, $"\"{filter.SearchTerm}\""));

        }


        // Specializations
        if (filter.specialization.HasValue)
            query = query.Where(c => c.Specialization == filter.specialization.Value);
        // Budget
        if (filter.MinBudget.HasValue)
            query = query.Where(c => c.Budget >= filter.MinBudget.Value);

        if (filter.MaxBudget.HasValue)
            query = query.Where(c => c.Budget <= filter.MaxBudget.Value);

        // Sorting
       
            query = (filter.Sort ?? string.Empty).ToLower() switch
            {
                "newest" => query.OrderByDescending(c => c.CreatedAt),
                "oldest" => query.OrderBy(c => c.CreatedAt),
                "budgetasc" => query.OrderBy(c => c.Budget),
                "budgetdesc" => query.OrderByDescending(c => c.Budget),
                _ => query.OrderByDescending(c => c.CreatedAt)
            };
        

        // Count after filters
        var totalCount = await query.CountAsync();
        if (totalCount == 0)
        {
            _logger.LogWarning("No consultations found");
            return _responseHandler.NotFound<PaginatedResult<GetConsultationResponse>>("No consultations found.");
        }

        // Pagination
        var consultations = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new GetConsultationResponse
            {
                Id = c.Id,
                Title = c.Title,
                ClientId = c.ClientId,
                Budget = c.Budget,
                Specialization = c.Specialization,
                CreatedAt = c.CreatedAt,
                Status = c.Status,
                Duration = c.Duration,
                UrlFiles = c.Files.Select(f => f.FilePath).ToList(),
                Description = c.Description,
                LawyerId = c.LawyerId,  
                Client = new ClientInfo
                {
                    Id = c.Client.Id,
                    ProfileImage = c.Client.ProfileImage.ImageUrl,
                    FullName = c.Client.FirstName + " " + c.Client.LastName,

                },
               ProposalsCount = c.Proposals.Count

            })
            .ToListAsync();

        var result = new PaginatedResult<GetConsultationResponse>
        {
            Items = consultations,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return _responseHandler.Success(result, "Consultations retrieved successfully");
    }


    // for Client and lawyer
    public async Task<Response<List<GetConsultationResponse>>> GetMyLatestConsultationsAsync()
    {
        try
        {
            var userId = ConsultationServiceHelper.GetCurrentUserId(_httpContextAccessor);
            if (string.IsNullOrEmpty(userId))
                return _responseHandler.Unauthorized<List<GetConsultationResponse>>("User not authenticated.");

            var consultations = await ConsultationServiceHelper.GetConsultationsAsync(
                _context,
                c => c.Client.Id == userId || c.LawyerId == userId,
                includeFiles: false,
                take: 5
            );

            var responses = consultations
                .Select(ConsultationServiceHelper.ToConsultationResponse)
                .ToList();

            return _responseHandler.Success(responses, "Latest consultations retrieved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving latest consultations.");
            return _responseHandler.ServerError<List<GetConsultationResponse>>("An error occurred while retrieving latest consultations.");
        }
    }
    public async Task<Response<List<GetConsultationResponse>>> GetMyConsultationsInProgressAsync()
    {
        try
        {
            var userId = ConsultationServiceHelper.GetCurrentUserId(_httpContextAccessor);
            if (string.IsNullOrEmpty(userId))
                return _responseHandler.Unauthorized<List<GetConsultationResponse>>("User not authenticated.");

            var consultations = await ConsultationServiceHelper.GetConsultationsAsync(
                _context,
                c => (c.Client.Id == userId || c.LawyerId == userId)
                    && c.Status == ConsultationStatus.InProgress,
                includeFiles: true
            );

            var responses = consultations
                .Select(ConsultationServiceHelper.ToConsultationResponse)
                .ToList();
            var count = responses.Count;

            return _responseHandler.Success(responses, "In-progress consultations retrieved successfully.");
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
    // for Client and lawyer
    public async Task<Response<List<ShowAllConsultaionWithoutDetails>>> GetMyConsultationsAsync()
    {
        try
        {
            var userId = ConsultationServiceHelper.GetCurrentUserId(_httpContextAccessor);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User not authenticated.");
                return _responseHandler.Unauthorized<List<ShowAllConsultaionWithoutDetails>>("User not authenticated.");
            }

            var clientConsultations = await ConsultationServiceHelper.GetConsultationsAsync(
                _context,
                c => c.Client.Id == userId,
                includeFiles: true
               
            );
            var lawyerConsultations = await ConsultationServiceHelper.GetConsultationsAsync(
                _context,
                c => c.Lawyer.Id == userId,
                includeFiles: true
                
            );
            var role = _httpContextAccessor.HttpContext.User.Claims
             .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(role))
            {
                return _responseHandler.Unauthorized<List<ShowAllConsultaionWithoutDetails>>("User role not found.");
            }

            List<ShowAllConsultaionWithoutDetails> consultationResponses;

            if (role == "Client")
            {
                consultationResponses = clientConsultations.Select(c => new ShowAllConsultaionWithoutDetails
                {
                    Id = c.Id,
                    Title = c.Title,
                    ClientId = c.ClientId,
                    ClientName = c.Client.FirstName + " " + c.Client.LastName,
                    Status = c.Status.ToString(),
                    Budget = c.Budget,
                }).ToList();

                return _responseHandler.Success(consultationResponses, "Consultations retrieved.");
            }
            else if (role == "Lawyer")
            {
                consultationResponses = lawyerConsultations.Select(c => new ShowAllConsultaionWithoutDetails
                {
                    Id = c.Id,
                    Title = c.Title,
                    ClientId = c.ClientId, 
                    ClientName = c.Client.FirstName + " " + c.Client.LastName,
                    Status = c.Status.ToString(),
                    Budget = c.Budget,
                }).ToList();

                return _responseHandler.Success(consultationResponses, "Consultations retrieved.");
            }
            else
            {
                return _responseHandler.Success(new List<ShowAllConsultaionWithoutDetails>(), "No consultations found.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all consultations.");
            return _responseHandler.ServerError<List<ShowAllConsultaionWithoutDetails>>("An error occurred while retrieving all consultations.");
        }
    }

    public async Task<Response<List<GetConsultationResponse>>> GetNewestOrders()
    {
        try
        {
            var lawyerId = ConsultationServiceHelper.GetCurrentUserId(_httpContextAccessor);
            if (string.IsNullOrEmpty(lawyerId))
                return _responseHandler.Unauthorized<List<GetConsultationResponse>>("User not authenticated.");

            var consultations = await ConsultationServiceHelper.GetConsultationsAsync(
                _context,
                c => c.LawyerId == lawyerId

                    && c.Status == ConsultationStatus.Active,
                includeFiles: true
            );

            var responses = consultations
                .OrderByDescending(c => c.CreatedAt)
                .Select(ConsultationServiceHelper.ToConsultationResponse)
                .ToList();

            return _responseHandler.Success(responses, "Newest orders retrieved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving newest orders.");
            return _responseHandler.ServerError<List<GetConsultationResponse>>("An error occurred while retrieving newest orders.");
        }
    }

    #region Helpers
    private async Task<(List<ConsultationFile> Files, List<string> Urls)> UploadFilesAsync(
    List<IFormFile> files,
    Guid consultationId,
    IImageUploadService imageUploadService)
    {
        var uploadedFiles = new List<ConsultationFile>();
        var uploadedUrls = new List<string>();

        if (files != null && files.Count > 0)
        {
            foreach (var file in files)
            {
                var uploadResult = await imageUploadService.UploadAsync(file);

                uploadedFiles.Add(new ConsultationFile
                {
                    ConsultationId = consultationId,
                    FileName = file.FileName,
                    FilePath = uploadResult.Url
                });

                uploadedUrls.Add(uploadResult.Url);
            }
        }

        return (uploadedFiles, uploadedUrls);
    }

    #endregion
}