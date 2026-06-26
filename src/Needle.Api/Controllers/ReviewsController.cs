using Microsoft.AspNetCore.Mvc;
using Needle.Api.Contracts.Reviews;
using Needle.Application.Reviews.CreateReview;

namespace Needle.Api.Controllers;

[ApiController]
[Route("api/albums/{albumId:guid}/reviews")]
public sealed class ReviewsController : ControllerBase
{
    private readonly CreateReviewHandler _createReviewHandler;

    public ReviewsController(CreateReviewHandler createReviewHandler)
    {
        ArgumentNullException.ThrowIfNull(createReviewHandler);

        _createReviewHandler = createReviewHandler;
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
}