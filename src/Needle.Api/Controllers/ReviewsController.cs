using Microsoft.AspNetCore.Mvc;
using Needle.Api.Contracts.Reviews;
using Needle.Application.Reviews.CreateReview;
using Needle.Application.Reviews.UpdateReview;

namespace Needle.Api.Controllers;

[ApiController]
[Route("api/albums/{albumId:guid}/reviews")]
public sealed class ReviewsController : ControllerBase
{
    private readonly CreateReviewHandler _createReviewHandler;
    private readonly UpdateReviewHandler _updateReviewHandler;

    public ReviewsController(
        CreateReviewHandler createReviewHandler,
        UpdateReviewHandler updateReviewHandler)
    {
        ArgumentNullException.ThrowIfNull(createReviewHandler);
        ArgumentNullException.ThrowIfNull(updateReviewHandler);

        _createReviewHandler = createReviewHandler;
        _updateReviewHandler = updateReviewHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid albumId,
        CreateReviewRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateReviewCommand(
            albumId,
            request.UserId,
            request.Rating,
            request.Text);

        var result = await _createReviewHandler.HandleAsync(
            command,
            cancellationToken);

        return result.Status switch
        {
            CreateReviewStatus.AlbumNotFound => NotFound(),

            CreateReviewStatus.AlreadyReviewed => Conflict(),

            CreateReviewStatus.Created => Created(
                $"/api/albums/{albumId}/reviews/{result.Review!.Id}",
                new CreateReviewResponse(
                    result.Review.Id,
                    result.Review.AlbumId,
                    result.Review.UserId,
                    result.Review.Rating.Value,
                    result.Review.Text,
                    result.Review.CreatedAt,
                    result.Review.UpdatedAt)),

            _ => throw new InvalidOperationException(
                $"Unexpected create review status: {result.Status}.")
        };
    }
    
    [HttpPut("{reviewId:guid}")]
    public async Task<IActionResult> Update(
        Guid albumId,
        Guid reviewId,
        UpdateReviewRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateReviewCommand(
            albumId,
            reviewId,
            request.UserId,
            request.Rating,
            request.Text);

        var result = await _updateReviewHandler.HandleAsync(
            command,
            cancellationToken);

        return result.Status switch
        {
            UpdateReviewStatus.ReviewNotFound => NotFound(),

            UpdateReviewStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden),

            UpdateReviewStatus.Updated => Ok(
                new UpdateReviewResponse(
                    result.Review!.Id,
                    result.Review.AlbumId,
                    result.Review.UserId,
                    result.Review.Rating.Value,
                    result.Review.Text,
                    result.Review.CreatedAt,
                    result.Review.UpdatedAt)),

            _ => throw new InvalidOperationException(
                $"Unexpected update review status: {result.Status}.")
        };
    }
}