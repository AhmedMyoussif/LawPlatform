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

namespace LawPlatform.DataAccess.Services.Admin
{
    public class AdminService:IAdminService
    {
        private readonly UserManager<User> _userManager;
        private readonly LawPlatformContext _context;
        private readonly ResponseHandler _responseHandler;

        public AdminService(UserManager<User> userManager, LawPlatformContext context, ResponseHandler responseHandler)
        {
           
            _context = context;
            _responseHandler = responseHandler;
            _userManager = userManager;
        }

   
        #region Get Lawyers by Status
        public async Task<Response<List<GetLawyerByStatusResponse>>> GetLawyersByStatusAsync(ApprovalStatus? status = null)
        {
            var query = _context.Lawyers
                .Where(l => !status.HasValue || l.Status == status.Value)
                .Join(_userManager.Users,
                      lawyer => lawyer.Id,
                      user => user.Id,
                      (lawyer, user) => new GetLawyerByStatusResponse
                      {
                          Id = lawyer.Id,
                          FullName = user.UserName,
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

        #region Update Lawyer Account Status
        public async Task<Response<UpdateLawyerAccountStatusResponse>> UpdateLawyerAccountStatusAsync(UpdateLawyerAccountStatusRequest model)
        {
            if (string.IsNullOrEmpty(model.LawyerId))
                return _responseHandler.BadRequest<UpdateLawyerAccountStatusResponse>("LawyerId is required.");

            var lawyer = await _context.Lawyers.FirstOrDefaultAsync(l => l.Id == model.LawyerId);
            if (lawyer == null)
                return _responseHandler.NotFound<UpdateLawyerAccountStatusResponse>("Lawyer not found.");


            await _context.SaveChangesAsync();

            var response = new UpdateLawyerAccountStatusResponse
            {
                LawyerId = lawyer.Id,
                Status = lawyer.Status,
            };

            return _responseHandler.Success(response, "Lawyer account status updated successfully.");
        }

        #endregion
    }
}
