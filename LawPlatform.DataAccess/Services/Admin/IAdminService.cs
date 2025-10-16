using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Entities.DTO.Account.Auth.Admin;
using LawPlatform.Entities.Shared.Bases;
using LawPlatform.Utilities.Enums;

namespace LawPlatform.DataAccess.Services.Admin
{
    public interface IAdminService
    {
        #region Get / Update Lawyer
        Task<Response<List<GetLawyerResponse>>> GetLawyersByStatusAsync(ApprovalStatus? status = null);
        Task<Response<UpdateLawyerAccountStatusResponse>> UpdateLawyerAccountStatusAsync(UpdateLawyerAccountStatusRequest model);
        Task<Response<GetLawyerResponse>> GetLawyerByIdAsync(string lawyerId);
        #endregion

        #region Get / Update Client

        Task<Response<List<GetClientsResponse>>> GetAllClients();
        Task<Response<GetClientsResponse>> GetClientById(string clientid);
        #endregion

        #region Delete / Delete Account
        Task<Response<bool>> DeleteAccountAsync(Guid userId, CancellationToken cancellationToken = default);
        #endregion

    }
}
