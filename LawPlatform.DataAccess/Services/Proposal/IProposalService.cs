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
        Task<Response<List<GetProposalResponse>>> GetProposalsByConsultationIdAsync(Guid consultationId);
        Task<Response<GetProposalResponse>> GetProposalByIdAsync(Guid proposalId); // For Lawyer who has this proposal , Client who post the consultation  
        Task<Response<AcceptProposalResponse>> AcceptProposalAsync(Guid proposalId); // Only Client who post the consultation
    }
}
