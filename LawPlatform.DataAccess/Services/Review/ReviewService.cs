using LawPlatform.DataAccess.ApplicationContext;
using LawPlatform.Entities.DTO.Review;
using LawPlatform.Entities.Shared.Bases;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LawPlatform.DataAccess.Services.Review;

public class ReviewService : IReviewService
{
    private readonly LawPlatformContext _context;
    private readonly ResponseHandler _responseHandler;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(
        LawPlatformContext context,
        ResponseHandler responseHandler,
        ILogger<ReviewService> logger)
    {
        _context = context;
        _responseHandler = responseHandler;
        _logger = logger;
    }
    
    public async Task<Response<bool>> AddReviewAsync(string userId, AddReviewRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting AddReviewAsync for LawyerId: {LawyerId}", request.LawyerId);

        try
        {
            // Verify the client exists
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.Id == userId && !c.IsDeleted, cancellationToken);

            if (client == null)
            {
                _logger.LogWarning("Only clients can submit reviews. UserId: {UserId}", userId);
                return _responseHandler.BadRequest<bool>("Only clients can submit reviews.");
            }

            // Verify the lawyer exists
            var lawyer = await _context.Lawyers
                .FirstOrDefaultAsync(l => l.Id == request.LawyerId.ToString(), cancellationToken);

            if (lawyer == null)
            {
                _logger.LogWarning("Lawyer not found. LawyerId: {LawyerId}", request.LawyerId);
                return _responseHandler.NotFound<bool>("Lawyer not found.");
            }

            // Check if client has already reviewed this lawyer
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ClientId == userId &&
                                          r.LawyerId == request.LawyerId.ToString() &&
                                          r.IsDeleted != true,
                                     cancellationToken);

            if (existingReview != null)
            {
                _logger.LogWarning("Client has already reviewed this lawyer. ClientId: {ClientId}, LawyerId: {LawyerId}",
                    userId, request.LawyerId);
                return _responseHandler.BadRequest<bool>("You have already reviewed this lawyer.");
            }

            // Check if client had any consultation with this lawyer
            var hadConsultation = await _context.consultations
                .AnyAsync(c => c.ClientId == userId &&
                               c.LawyerId == request.LawyerId.ToString(),
                           cancellationToken);

            if (!hadConsultation)
            {
                _logger.LogWarning("Client has not had any consultation with this lawyer. ClientId: {ClientId}, LawyerId: {LawyerId}",
                    userId, request.LawyerId);
                return _responseHandler.BadRequest<bool>("You can only review lawyers you have had consultations with.");
            }

            // Create the review
            var review = new Entities.Models.Review
            {
                Id = Guid.NewGuid(),
                Comment = request.Comment,
                Rating = request.Rating,
                LawyerId = request.LawyerId.ToString(),
                ClientId = userId,
            };

            await _context.Reviews.AddAsync(review, cancellationToken);

            lawyer.TotalReviews += 1;
            lawyer.Rating = (await GetAverageRatingForLawyerAsync(Guid.Parse(lawyer.Id))).Data;
            _context.Lawyers.Update(lawyer);
            await _context.SaveChangesAsync(cancellationToken);


            _logger.LogInformation("Review added successfully. ReviewId: {ReviewId}, LawyerId: {LawyerId}, ClientId: {ClientId}",
                review.Id, request.LawyerId, userId);

            return _responseHandler.Success(true, "Review added successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding review for LawyerId: {LawyerId}", request.LawyerId);
            return _responseHandler.ServerError<bool>("An error occurred while adding the review.");
        }
    }

    public async Task<Response<double>> GetAverageRatingForLawyerAsync(Guid lawyerId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting GetAverageRatingForLawyerAsync for LawyerId: {LawyerId}", lawyerId);

        try
        {
            // Verify the lawyer exists
            var lawyer = await _context.Lawyers
                .FirstOrDefaultAsync(l => l.Id == lawyerId.ToString(), cancellationToken);

            if (lawyer == null)
            {
                _logger.LogWarning("Lawyer not found. LawyerId: {LawyerId}", lawyerId);
                return _responseHandler.NotFound<double>("Lawyer not found.");
            }

            // Get all non-deleted reviews for the lawyer
            var reviews = await _context.Reviews
                .Where(r => r.LawyerId == lawyerId.ToString() && r.IsDeleted != true)
                .ToListAsync(cancellationToken);

            if (!reviews.Any())
            {
                _logger.LogInformation("No reviews found for LawyerId: {LawyerId}", lawyerId);
                return _responseHandler.Success(0.0, "No reviews found for this lawyer.");
            }

            // Calculate average rating
            var averageRating = reviews.Average(r => r.Rating);

            _logger.LogInformation("Average rating calculated for LawyerId: {LawyerId}, Rating: {Rating}", 
                lawyerId, averageRating);

            return _responseHandler.Success(Math.Round(averageRating, 2), "Average rating retrieved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting average rating for LawyerId: {LawyerId}", lawyerId);
            return _responseHandler.ServerError<double>("An error occurred while retrieving the average rating.");
        }
    }

    public async Task<Response<List<ReviewResponse>>> GetReviewsForLawyerAsync(Guid lawyerId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting GetReviewsForLawyerAsync for LawyerId: {LawyerId}", lawyerId);

        try
        {
            // Verify the lawyer exists
            var lawyer = await _context.Lawyers
                .FirstOrDefaultAsync(l => l.Id == lawyerId.ToString(), cancellationToken);

            if (lawyer == null)
            {
                _logger.LogWarning("Lawyer not found. LawyerId: {LawyerId}", lawyerId);
                return _responseHandler.NotFound<List<ReviewResponse>>("Lawyer not found.");
            }

            // Get all non-deleted reviews for the lawyer
            var reviews = await _context.Reviews
                .Where(r => r.LawyerId == lawyerId.ToString() && r.IsDeleted != true)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewResponse(
                    r.Comment,
                    r.Rating,
                    Guid.Parse(r.LawyerId),
                    Guid.Parse(r.ClientId)
                ))
                .ToListAsync(cancellationToken);

            if (!reviews.Any())
            {
                _logger.LogInformation("No reviews found for LawyerId: {LawyerId}", lawyerId);
                return _responseHandler.Success(new List<ReviewResponse>(), "No reviews found for this lawyer.");
            }

            _logger.LogInformation("Retrieved {Count} reviews for LawyerId: {LawyerId}", reviews.Count, lawyerId);

            return _responseHandler.Success(reviews, "Reviews retrieved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting reviews for LawyerId: {LawyerId}", lawyerId);
            return _responseHandler.ServerError<List<ReviewResponse>>("An error occurred while retrieving reviews.");
        }
    }

    public async Task<Response<bool>> UpdateReviewAsync(Guid reviewId, string userId, UpdateReviewRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting UpdateReviewAsync for ReviewId: {ReviewId}", reviewId);

        try
        {
            // Verify the client exists
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.Id == userId, cancellationToken);

            if (client == null)
            {
                _logger.LogWarning("Only clients can update reviews. UserId: {UserId}", userId);
                return _responseHandler.BadRequest<bool>("Only clients can update reviews.");
            }

            // Find the review
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.IsDeleted != true, cancellationToken);

            if (review == null)
            {
                _logger.LogWarning("Review not found or already deleted. ReviewId: {ReviewId}", reviewId);
                return _responseHandler.NotFound<bool>("Review not found or already deleted.");
            }

            // Verify the review belongs to the client
            if (review.ClientId != userId)
            {
                _logger.LogWarning("Client does not own this review. ClientId: {ClientId}, ReviewId: {ReviewId}", 
                    userId, reviewId);
                return _responseHandler.Forbidden<bool>("You can only update your own reviews.");
            }

            // Validate rating range
            if (request.Rating < 1 || request.Rating > 5)
            {
                _logger.LogWarning("Invalid rating value: {Rating}", request.Rating);
                return _responseHandler.BadRequest<bool>("Rating must be between 1 and 5.");
            }

            // Update the review
            review.Comment = request.Comment;
            review.Rating = request.Rating;
            review.UpdatedAt = DateTime.UtcNow;

            _context.Reviews.Update(review);

            // Update lawyer's average rating
            var lawyer = await _context.Lawyers
                .FirstOrDefaultAsync(l => l.Id == review.LawyerId && !l.IsDeleted, cancellationToken);

            if (lawyer == null) { 
                _logger.LogWarning("Lawyer not found or deleted for review. LawyerId: {LawyerId}", review.LawyerId);
                return _responseHandler.NotFound<bool>("Associated lawyer not found.");
            }

            lawyer.Rating = (await GetAverageRatingForLawyerAsync(Guid.Parse(lawyer.Id))).Data;
            _context.Lawyers.Update(lawyer);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Review updated successfully. ReviewId: {ReviewId}, ClientId: {ClientId}", 
                reviewId, userId);

            return _responseHandler.Success(true, "Review updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating review. ReviewId: {ReviewId}", reviewId);
            return _responseHandler.ServerError<bool>("An error occurred while updating the review.");
        }
    }

    public async Task<Response<bool>> DeleteReviewAsync(Guid reviewId, string clientId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting DeleteReviewAsync for ReviewId: {ReviewId}", reviewId);

        try
        {
            // Verify the client exists
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.Id == clientId, cancellationToken);

            if (client == null)
            {
                _logger.LogWarning("Only clients can delete reviews. UserId: {UserId}", clientId);
                return _responseHandler.BadRequest<bool>("Only clients can delete reviews.");
            }

            // Find the review
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.IsDeleted != true, cancellationToken);

            if (review == null)
            {
                _logger.LogWarning("Review not found or already deleted. ReviewId: {ReviewId}", reviewId);
                return _responseHandler.NotFound<bool>("Review not found or already deleted.");
            }

            // Verify the review belongs to the client
            if (review.ClientId != clientId)
            {
                _logger.LogWarning("Client does not own this review. ClientId: {ClientId}, ReviewId: {ReviewId}", 
                    clientId, reviewId);
                return _responseHandler.Forbidden<bool>("You can only delete your own reviews.");
            }

            // Soft delete the review
            review.IsDeleted = true;
            review.DeletedAt = DateTime.UtcNow;

            _context.Reviews.Update(review);

            // Update lawyer's average rating
            var lawyer = await _context.Lawyers
                .FirstOrDefaultAsync(l => l.Id == review.LawyerId && !l.IsDeleted, cancellationToken);

            if (lawyer == null)
            {
                _logger.LogWarning("Lawyer not found or deleted for review. LawyerId: {LawyerId}", review.LawyerId);
                return _responseHandler.NotFound<bool>("Associated lawyer not found.");
            }
            lawyer.TotalReviews -= 1 ;
            lawyer.Rating = (await GetAverageRatingForLawyerAsync(Guid.Parse(lawyer.Id))).Data;
            _context.Lawyers.Update(lawyer);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Review soft deleted successfully. ReviewId: {ReviewId}, ClientId: {ClientId}", 
                reviewId, clientId);

            return _responseHandler.Success(true, "Review deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting review. ReviewId: {ReviewId}", reviewId);
            return _responseHandler.InternalServerError<bool>("An error occurred while deleting the review.");
        }
    }
}