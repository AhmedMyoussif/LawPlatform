using Ecommerce.API.Extensions;
using LawPlatform.DataAccess.Services.Review;
using LawPlatform.Entities.DTO.Review;
using LawPlatform.Entities.Shared.Bases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LawPlatform.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReviewController(
    ILogger<ReviewController> logger,
    ResponseHandler responseHandler,
    IReviewService reviewService) : ControllerBase
{
    private readonly ILogger<ReviewController> _logger = logger;
    private readonly ResponseHandler _responseHandler = responseHandler;
    private readonly IReviewService _reviewService = reviewService;

    [HttpPost("")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> AddReview([FromBody] AddReviewRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AddReview endpoint called");

        // Get current logged-in user
        var userId = User.GetUserId();

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User not authenticated.");
            var errorResponse = _responseHandler.Unauthorized<bool>("User not authenticated.");
            return StatusCode((int)errorResponse.StatusCode, errorResponse);
        }

        var result = await _reviewService.AddReviewAsync(userId, request, cancellationToken);

        return StatusCode((int)result.StatusCode, result);
    }

    [HttpGet("lawyer/{lawyerId}/average-rating")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAverageRatingForLawyer([FromRoute] Guid lawyerId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetAverageRatingForLawyer endpoint called for LawyerId: {LawyerId}", lawyerId);

        var result = await _reviewService.GetAverageRatingForLawyerAsync(lawyerId, cancellationToken);

        return StatusCode((int)result.StatusCode, result);
    }

    [HttpGet("lawyer/{lawyerId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetReviewsForLawyer([FromRoute] Guid lawyerId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetReviewsForLawyer endpoint called for LawyerId: {LawyerId}", lawyerId);

        var result = await _reviewService.GetReviewsForLawyerAsync(lawyerId, cancellationToken);

        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPut("{reviewId}")]
    [Authorize(Roles="Client")]
    public async Task<IActionResult> UpdateReview([FromRoute] Guid reviewId, [FromBody] UpdateReviewRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("UpdateReview endpoint called for ReviewId: {ReviewId}", reviewId);

        // Get current logged-in user
        var userId = User.GetUserId();

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User not authenticated.");
            var errorResponse = _responseHandler.Unauthorized<bool>("User not authenticated.");
            return StatusCode((int)errorResponse.StatusCode, errorResponse);
        }

        var result = await _reviewService.UpdateReviewAsync(reviewId, userId, request, cancellationToken);

        return StatusCode((int)result.StatusCode, result);
    }

    [HttpDelete("{reviewId}")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> DeleteReview([FromRoute] Guid reviewId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("DeleteReview endpoint called for ReviewId: {ReviewId}", reviewId);

        // Get current logged-in user
        var userId = User.GetUserId();

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User not authenticated.");
            var errorResponse = _responseHandler.Unauthorized<bool>("User not authenticated.");
            return StatusCode((int)errorResponse.StatusCode, errorResponse);
        }

        var result = await _reviewService.DeleteReviewAsync(reviewId, userId, cancellationToken);

        return StatusCode((int)result.StatusCode, result);
    }
}
