using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Entities.DTO.Account.Auth.Admin;
using LawPlatform.Entities.DTO.Consultaion;
using LawPlatform.Entities.Shared.Bases;
using LawPlatform.Utilities.Enums;

namespace LawPlatform.DataAccess.Services.Admin
{
    public interface IAdminService
    {
        #region Get / Update Lawyer
        Task<Response<PaginatedResult<GetLawyerResponse>>> GetLawyersByStatusAsync(ApprovalStatus? status = null , int pageNumber = 1, int pageSize = 10);
        Task<Response<UpdateLawyerAccountStatusResponse>> UpdateLawyerAccountStatusAsync(UpdateLawyerAccountStatusRequest model);
        Task<Response<GetLawyerResponse>> GetLawyerByIdAsync(string lawyerId);
        #endregion

        #region Get /  Client

        Task<Response<List<GetClientsResponse>>> GetAllClients(string? search);
        Task<Response<GetClientsResponse>> GetClientById(string clientid);
        #endregion

        #region Delete / Delete Account
        Task<Response<bool>> DeleteAccountAsync(Guid userId, CancellationToken cancellationToken = default);
        #endregion

        #region Mentoring 
        Task<Response<PaginatedResult<ShowAllConsultaionWithoutDetails>>> MentorConsultationsync( string consultation , int pageNumber = 1 , int pageSize = 10);
        #endregion

        #region Statistics 
        Task<Response<int>> GetTotalConsultationsCountAsync();
        Task<Response<int>> GetTotalClientsCountAsync();
        #endregion

    }
}
