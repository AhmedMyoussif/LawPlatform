using LawPlatform.Entities.DTO.Review;
using LawPlatform.Entities.Shared.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPlatform.DataAccess.Services.Review;

public interface IReviewService
{
    Task<Response<bool>> AddReviewAsync(string userId, AddReviewRequest request, CancellationToken cancellationToken = default);
    Task<Response<double>> GetAverageRatingForLawyerAsync(Guid lawyerId, CancellationToken cancellationToken = default);
    Task<Response<List<ReviewResponse>>> GetReviewsForLawyerAsync(Guid lawyerId, CancellationToken cancellationToken = default);

    Task<Response<bool>> UpdateReviewAsync(Guid reviewId, string userId, UpdateReviewRequest request, CancellationToken cancellationToken = default);
    Task<Response<bool>> DeleteReviewAsync(Guid reviewId, string clientId, CancellationToken cancellationToken = default);


}
