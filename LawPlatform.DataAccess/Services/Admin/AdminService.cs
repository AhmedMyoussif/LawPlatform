using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.DataAccess.ApplicationContext;
using LawPlatform.Entities.DTO.Account.Auth.Admin;
using LawPlatform.Entities.Models.Auth.Identity;
using LawPlatform.Entities.Shared.Bases;
using LawPlatform.Utilities.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LawPlatform.DataAccess.Services.Admin
{
    public class AdminService:IAdminService
    {
        private readonly UserManager<User> _userManager;
        private readonly LawPlatformContext _context;
        private readonly ResponseHandler _responseHandler;
        private readonly ILogger<AdminService> _logger;

        public AdminService(UserManager<User> userManager, LawPlatformContext context, ResponseHandler responseHandler, ILogger<AdminService> logger)
        {

            _context = context;
            _responseHandler = responseHandler;
            _userManager = userManager;
            _logger = logger;
        }


        #region Get Lawyers by Status
        public async Task<Response<List<GetLawyerResponse>>> GetLawyersByStatusAsync(ApprovalStatus? status = ApprovalStatus.Pending)
        {
            var query = _context.Lawyers
                .Where(l => !status.HasValue || l.Status == status.Value)
                .Join(_userManager.Users,
                      lawyer => lawyer.UserId,
                      user => user.Id,
                      (lawyer, user) => new GetLawyerResponse
                      {
                          Id = lawyer.Id,
                          FirstName = lawyer.FirstName,
                          LastName = lawyer.LastName,
                          UserName = user.UserName,
                          Email = user.Email,
                          PhoneNumber = user.PhoneNumber,
                          QualificationDocumentUrl = lawyer.QualificationDocumentPath,
                          Status = lawyer.Status,
                          CreatedAt = lawyer.CreatedAt                         
                      });

            var lawyers = await query.ToListAsync();
            return _responseHandler.Success(lawyers, "Lawyers retrieved successfully.");
        }
        #endregion
        #region Get Lawyer by Id

        public async Task<Response<GetLawyerResponse>> GetLawyerByIdAsync(string lawyerId)
        {
            if (string.IsNullOrEmpty(lawyerId))
                return _responseHandler.BadRequest<GetLawyerResponse>("LawyerId is required.");
            var lawyer = await _context.Lawyers
                .Where(l => l.Id == lawyerId)
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
                          CreatedAt = l.CreatedAt
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

            return _responseHandler.Success(response, "Lawyer account status updated successfully.");
        }


        #endregion

        #region Get /  Client
        public async Task<Response<List<GetClientsResponse>>> GetAllClients()
        {
            _logger.LogInformation("Starting GetAllClients at {Time}", DateTime.UtcNow);

            try
            {
                var clients = await _context.Clients
                    .Join(_userManager.Users,
                        client => client.Id,
                        user => user.Id,
                        (client, user) => new GetClientsResponse
                        {
                            Id = client.Id,
                            FullName = user.UserName,
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber,
                            CreatedAt = client.CreatedAt,
                            ConsultationCount = client.Consultations.Count(),

                        })
                    .ToListAsync();

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
                    .Where(c => c.Id == clientId)
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
    }
}
