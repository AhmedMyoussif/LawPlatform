using LawPlatform.DataAccess.ApplicationContext;
using LawPlatform.DataAccess.Services.Email;
using LawPlatform.DataAccess.Services.Notification;
using LawPlatform.Entities.DTO.Account.Auth.Admin;
using LawPlatform.Entities.DTO.Consultation;
using LawPlatform.Entities.DTO.Shared;
using LawPlatform.Entities.Models.Auth.Identity;
using LawPlatform.Entities.Shared;
using LawPlatform.Entities.Shared.Bases;
using LawPlatform.Utilities.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace LawPlatform.DataAccess.Services.Admin
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<User> _userManager;
        private readonly LawPlatformContext _context;
        private readonly ResponseHandler _responseHandler;
        private readonly ILogger<AdminService> _logger;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;


        public AdminService(UserManager<User> userManager, LawPlatformContext context, ResponseHandler responseHandler, ILogger<AdminService> logger, INotificationService notificationService, IEmailService emailService)
        {

            _context = context;
            _responseHandler = responseHandler;
            _userManager = userManager;
            _logger = logger;
            _notificationService = notificationService;
            _emailService = emailService;
        }


        #region Get Lawyers by Status
        public async Task<Response<PaginatedList<GetLawyerBriefResponse>>> GetLawyersByStatusAsync(ApprovalStatus? status, RequestFilters<LawyerSorting> filters)
        {
            try
            {
                var query = _context.Lawyers
                    .Where(l => !l.IsDeleted && (!status.HasValue || l.Status == status.Value))
                    .Include(l => l.ProfileImage)
                    .Select(lawyer => new GetLawyerBriefResponse
                    {
                        Id = lawyer.Id,
                        FirstName = lawyer.FirstName,
                        LastName = lawyer.LastName,
                        Status = lawyer.Status,
                        Email = lawyer.User.Email,
                        PhoneNumber = lawyer.User.PhoneNumber,
                        UserName = lawyer.User.UserName,
                        Age = lawyer.Age,
                        Address = lawyer.Address,
                        Bio = lawyer.Bio,
                        BankName = lawyer.BankName,
                        LicenseDocument = lawyer.LicenseDocumentPath,
                        LicenseNumber = lawyer.LicenseNumber,
                        Qualifications = lawyer.QualificationDocumentPath,
                        YersOfExperience = lawyer.YearsOfExperience,
                        Specialization = lawyer.Specialization.ToString(),
                        Experiences = lawyer.Experiences ?? "Not specified",
                        YearsOfExperience = lawyer.YearsOfExperience,
                        Rating = lawyer.Rating ?? 0.0,
                        CreatedAt = lawyer.CreatedAt.ToString("yyyy-MM-dd"),
                        ProfileImageUrl = lawyer.ProfileImage != null ? lawyer.ProfileImage.ImageUrl : null,
                        CompletedConsultations = lawyer.Consultations.Count(c => c.Status == ConsultationStatus.Completed)
                        
                    })
                    .AsNoTracking();

                // Apply sorting
                var isAscending = filters.SortDirection == SortDirection.ASC;
                query = filters.SortColumn switch
                {
                    LawyerSorting.Experience => isAscending ? query.OrderBy(l => l.YearsOfExperience) : query.OrderByDescending(l => l.YearsOfExperience),
                    LawyerSorting.Rating => isAscending ? query.OrderBy(l => l.Rating) : query.OrderByDescending(l => l.Rating),
                    LawyerSorting.CreatedAt => isAscending ? query.OrderBy(l => l.CreatedAt) : query.OrderByDescending(l => l.CreatedAt),
                    _ => query.OrderByDescending(l => l.Rating),
                };

                //Apply Searching
                query = string.IsNullOrWhiteSpace(filters.SearchValue)
                    ? query
                    : query.Where(l =>
                        EF.Functions.Contains(l.FirstName, $"\"{filters.SearchValue}\"") ||
                        EF.Functions.Contains(l.LastName, $"\"{filters.SearchValue}\"")
                    );


                //Apply Pagination
                var result = await PaginatedList<GetLawyerBriefResponse>.CreateAsync(query, filters.PageNumber, filters.PageSize);

                return _responseHandler.Success(result, "Lawyers retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving lawyers by status.");
                return _responseHandler.BadRequest<PaginatedList<GetLawyerBriefResponse>>("An error occurred while retrieving lawyers.");
            }
        }

        #endregion
        
        #region Get Lawyer by Id

        public async Task<Response<GetLawyerResponse>> GetLawyerByIdAsync(string lawyerId)
        {
            if (string.IsNullOrEmpty(lawyerId))
                return _responseHandler.BadRequest<GetLawyerResponse>("LawyerId is required.");
            var lawyer = await _context.Lawyers
                .Where(l => l.Id == lawyerId && !l.IsDeleted)
                .Join(_userManager.Users,
                      l => l.Id,
                      u => u.Id,
                      (l, u) => new GetLawyerResponse
                      {
                          Id = l.Id,
                          UserName = u.UserName,
                          FirstName = l.FirstName,
                          LastName = l.LastName,
                          Email = u.Email,
                          PhoneNumber = u.PhoneNumber,
                          QualificationDocumentUrl = l.QualificationDocumentPath,
                          Status = l.Status,
                          CreatedAt = l.CreatedAt,
                          Specialization = l.Specialization.ToString(),
                          Experiences = l.Experiences ?? "Not specified",
                          Bio = l.Bio,
                          Address = l.Address,
                          BankName = l.BankName,
                          YearsOfExperience = l.YearsOfExperience,
                          LicenseNumber = l.LicenseNumber,
                          LicenseDocumentUrl = l.LicenseDocumentPath,
                          Country = l.Country,
                          Age = l.Age,
                          ProfileImage = l.ProfileImage != null ? l.ProfileImage.ImageUrl : null,
                          IBAN = l.IBAN,
                          BankAccountNumber = l.BankAccountNumber
                         
                      })
                .FirstOrDefaultAsync();
            if (lawyer == null)
                return _responseHandler.NotFound<GetLawyerResponse>("Lawyer not found.");
            return _responseHandler.Success(lawyer, "Lawyer retrieved successfully.");
        }
        #endregion

        #region Update Lawyer Account Status
        public async Task<Response<UpdateLawyerAccountStatusResponse>> UpdateLawyerAccountStatusAsync(UpdateLawyerAccountStatusRequest model)
        {
            if (string.IsNullOrEmpty(model.LawyerId))
                return _responseHandler.BadRequest<UpdateLawyerAccountStatusResponse>("LawyerId is required.");

            var lawyer = await _context.Lawyers
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == model.LawyerId);

            if (lawyer == null)
                return _responseHandler.NotFound<UpdateLawyerAccountStatusResponse>("Lawyer not found.");

            lawyer.Status = model.ApprovalStatus;

            await _context.SaveChangesAsync();

            var response = new UpdateLawyerAccountStatusResponse
            {
                LawyerId = lawyer.Id,
                FullName = lawyer.FirstName + " " + lawyer.LastName,
                Email = lawyer.User.Email,
                PhoneNumber = lawyer.User.PhoneNumber,
                Status = lawyer.Status,
            };
            
            if (lawyer.Status == ApprovalStatus.Approved)
            {
                await _emailService.SendLawyerEmailAsync(lawyer, LawyerEmailType.Approved);
                await _notificationService.NotifyUserAsync(
                    lawyer.Id,
                    "Account Approved",
                    "Your lawyer account has been approved. You can now log in."
                );
            }

            if (lawyer.Status == ApprovalStatus.Rejected)
            {
                await _emailService.SendLawyerEmailAsync(lawyer, LawyerEmailType.Rejected);
                await _notificationService.NotifyUserAsync(
                    lawyer.Id,
                    "Account Rejected",
                    "Sorry, your lawyer account has been rejected by admin."
                );
            }


            return _responseHandler.Success(response, "Lawyer account status updated successfully.");
        }


        #endregion

        #region Get / Client
        public async Task<Response<List<GetClientsResponse>>> GetAllClients(string? search)
        {
            _logger.LogInformation("Starting GetAllClients at {Time}", DateTime.UtcNow);

            try
            {
                var query = _context.Clients
                    .Where(c => !c.IsDeleted)
                    .Join(_userManager.Users,
                          client => client.Id,
                          user => user.Id,
                          (client, user) => new GetClientsResponse
                          {
                              Id = client.Id,
                              FullName = client.FirstName + " " + client.LastName,
                              Email = user.Email,
                              PhoneNumber = user.PhoneNumber,
                              CreatedAt = client.CreatedAt,
                              ConsultationCount = client.Consultations.Count(),
                          });

                // Apply search filter before executing query
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(c =>
                        c.FullName.Contains(search) ||
                        c.Email.Contains(search) ||
                        c.PhoneNumber.Contains(search)
                    );
                }

                var clients = await query.ToListAsync();

                if (clients == null || clients.Count == 0)
                {
                    _logger.LogWarning("No clients found.");
                    return _responseHandler.Success(new List<GetClientsResponse>(), "No clients found.");
                }

                _logger.LogInformation("Retrieved {Count} clients.", clients.Count);

                return _responseHandler.Success(clients, "Clients retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving clients.");
                return _responseHandler.BadRequest<List<GetClientsResponse>>("An error occurred while retrieving clients.");
            }
        }

        public async Task<Response<GetClientsResponse>> GetClientById(string clientId)
        {
            _logger.LogInformation("Starting GetClientById for {ClientId} at {Time}", clientId, DateTime.UtcNow);

            if (string.IsNullOrEmpty(clientId))
                return _responseHandler.BadRequest<GetClientsResponse>("ClientId is required.");

            try
            {
                var client = await _context.Clients
                    .Where(c => c.Id == clientId && !c.IsDeleted)
                    .Join(_userManager.Users,
                          c => c.Id,
                          u => u.Id,
                          (c, u) => new GetClientsResponse
                          {
                              Id = c.Id,
                              FullName = u.UserName,
                              Email = u.Email,
                              PhoneNumber = u.PhoneNumber,
                              CreatedAt = c.CreatedAt,
                              ConsultationCount = c.Consultations.Count(),
                          })
                    .FirstOrDefaultAsync();

                if (client == null)
                {
                    _logger.LogWarning("Client with id {ClientId} not found.", clientId);
                    return _responseHandler.NotFound<GetClientsResponse>("Client not found.");
                }

                _logger.LogInformation("Retrieved client {ClientId}", clientId);

                return _responseHandler.Success(client, "Client retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving client {ClientId}", clientId);
                return _responseHandler.BadRequest<GetClientsResponse>("An error occurred while retrieving client.");
            }
        }

        #endregion

        #region Delete / Account

        public async Task<Response<bool>> DeleteAccountAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                _logger.LogWarning("User with id {UserId} not found.", userId);
                return _responseHandler.NotFound<bool>("User not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Mark related entities as deleted based on user role
                if (roles != null && roles.Count > 0)
                {
                    if (roles.Contains("Lawyer"))
                    {
                        var lawyer = await _context.Lawyers.FirstOrDefaultAsync(l => l.Id == userId.ToString(), cancellationToken);
                        if (lawyer == null)
                        {
                            _logger.LogWarning("Lawyer with id {UserId} not found.", userId);
                            return _responseHandler.NotFound<bool>("Lawyer not found.");
                        }
                        lawyer.IsDeleted = true;
                        lawyer.DeletedAt = DateTime.UtcNow;
                    }
                    else if (roles.Contains("Client"))
                    {
                        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == userId.ToString(), cancellationToken);
                        if (client == null)
                        {
                            _logger.LogWarning("Client with id {UserId} not found.", userId);
                            return _responseHandler.NotFound<bool>("Client not found.");
                        }
                        client.IsDeleted = true;
                        client.DeletedAt = DateTime.UtcNow;
                    }
                }


                // Mark refresh tokens as deleted/revoked
                var refreshTokens = await _context.UserRefreshTokens
                    .Where(rt => rt.UserId == userId.ToString())
                    .ToListAsync(cancellationToken);

                _context.UserRefreshTokens.RemoveRange(refreshTokens);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("User account {UserId} and related data marked as deleted.", userId);
                return _responseHandler.Success(true, "User account deleted successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "An error occurred while deleting user {UserId}", userId);
                return _responseHandler.InternalServerError<bool>("An error occurred while deleting user.");
            }
        }


        #endregion

        #region Monitor Consultations
        public async Task<Response<PaginatedResult<ShowAllConsultaionWithoutDetails>>> MonitorConsultationsAsync(
            string consultation, int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<ShowAllConsultaionWithoutDetails>>("Invalid pagination parameters.");

            var query = _context.consultations
                .Include(c => c.Client)
                .Include(c => c.Lawyer)
                .Where(c => c.Status != ConsultationStatus.Active)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(consultation))
            {
                query = query.Where(c => EF.Functions.Contains(c.Title, $"\"{consultation}\""));
            }

            var totalCount = await query.CountAsync();

            var consultations = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ShowAllConsultaionWithoutDetails
                {
                    Id = c.Id,
                    Title = c.Title,
                    Status = c.Status.ToString(),
                    CreatedAt = c.CreatedAt,
                    LawyerName = c.Lawyer != null ? c.Lawyer.FirstName + " " + c.Lawyer.LastName : "Unassigned",
                    ClientName = c.Client.FirstName + " " + c.Client.LastName,
                    Budget = c.Budget,
                    LawyerId = c.LawyerId,
                    ClientId = c.ClientId,
                    Specialization = c.Specialization.ToString()
                })
                .ToListAsync();

            var result = new PaginatedResult<ShowAllConsultaionWithoutDetails>
            {
                Items = consultations,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return _responseHandler.Success(result, "Consultations retrieved successfully.");
        }


        #endregion

        #region Statistics
        // count of clients 

        public async Task<Response<int>> GetTotalClientsCountAsync()
        {
            try
            {
                var count = await _context.Clients.Where(c => !c.IsDeleted).CountAsync();
                return _responseHandler.Success(count, "Total clients count retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving total clients count.");
                return _responseHandler.BadRequest<int>("An error occurred while retrieving total clients count.");
            }
        }

        // count of in progress consultations
        public async Task<Response<int>> GetTotalConsultationsCountAsync()
        {
            try
            {
                var count = await _context.consultations.Where(c => c.Status == ConsultationStatus.InProgress).CountAsync();
                return _responseHandler.Success(count, "Total consultations count retrieved successfully.");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving total consultations count.");
                return _responseHandler.BadRequest<int>("An error occurred while retrieving total consultations count.");
            }
        }

        #endregion

    }
}
