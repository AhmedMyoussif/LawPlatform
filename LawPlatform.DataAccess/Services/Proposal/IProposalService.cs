using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Entities.DTO.Proposal;
using LawPlatform.Entities.Shared.Bases;

namespace LawPlatform.DataAccess.Services.Proposal
{
    public interface IProposalService
    {
        Task<Response<GetProposalResponse>>SubmitProposalAsync(SubmitPropsalRequest dto);
    }
}
