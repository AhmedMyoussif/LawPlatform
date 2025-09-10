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
        Task<Response<List<GetLawyerByStatusResponse>>> GetLawyersByStatusAsync(ApprovalStatus? status = null);
        Task<Response<UpdateLawyerAccountStatusResponse>> UpdateLawyerAccountStatusAsync(UpdateLawyerAccountStatusRequest model);
        #endregion

    }
}
