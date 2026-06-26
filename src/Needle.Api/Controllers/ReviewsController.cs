using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Needle.Api.Contracts.Reviews;
using Needle.Application.Reviews.CreateReview;
using Needle.Application.Reviews.DeleteReview;
using Needle.Application.Reviews.GetReviewById;
using Needle.Application.Reviews.ListReviewsByAlbum;
using Needle.Application.Reviews.UpdateReview;

namespace Needle.Api.Controllers;

[ApiController]
[Route("api/albums/{albumId:guid}/reviews")]
public sealed class ReviewsController : ControllerBase
{
    private readonly CreateReviewHandler _createReviewHandler;
    private readonly UpdateReviewHandler _updateReviewHandler;
    private readonly ListReviewsByAlbumHandler _listReviewsByAlbumHandler;
    private readonly GetReviewByIdHandler _getReviewByIdHandler;
    private readonly DeleteReviewHandler _deleteReviewHandler;

    public ReviewsController(
        CreateReviewHandler createReviewHandler,
        UpdateReviewHandler updateReviewHandler,
        ListReviewsByAlbumHandler listReviewsByAlbumHandler,
        GetReviewByIdHandler getReviewByIdHandler,
        DeleteReviewHandler deleteReviewHandler)
    {
        ArgumentNullException.ThrowIfNull(createReviewHandler);
        ArgumentNullException.ThrowIfNull(updateReviewHandler);
        ArgumentNullException.ThrowIfNull(listReviewsByAlbumHandler);
        ArgumentNullException.ThrowIfNull(getReviewByIdHandler);
        ArgumentNullException.ThrowIfNull(deleteReviewHandler);

        _createReviewHandler = createReviewHandler;
        _updateReviewHandler = updateReviewHandler;
        _listReviewsByAlbumHandler = listReviewsByAlbumHandler;
        _getReviewByIdHandler = getReviewByIdHandler;
        _deleteReviewHandler = deleteReviewHandler;
    }

    /// <summary>
    /// Creates a review for an album.
    /// </summary>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(CreateReviewResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        Guid albumId,
        CreateReviewRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateReviewCommand(
            albumId,
            GetAuthenticatedUserId(),
            request.Rating,
            request.Text);

        var result = await _createReviewHandler.HandleAsync(
            command,
            cancellationToken);

        return result.Status switch
        {
            CreateReviewStatus.AlbumNotFound => NotFound(),

            CreateReviewStatus.AlreadyReviewed => Conflict(),

            CreateReviewStatus.Created => CreatedAtAction(
                nameof(GetById),
                new
                {
                    albumId,
                    reviewId = result.Review!.Id
                },
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

    /// <summary>
    /// Lists reviews for an album.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ListReviewsByAlbumResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListByAlbum(
        Guid albumId,
        CancellationToken cancellationToken)
    {
        var result = await _listReviewsByAlbumHandler.HandleAsync(
            new ListReviewsByAlbumQuery(albumId),
            cancellationToken);

        return result.Status switch
        {
            ListReviewsByAlbumStatus.AlbumNotFound => NotFound(),

            ListReviewsByAlbumStatus.Found => Ok(
                new ListReviewsByAlbumResponse(
                    result.Reviews
                        .Select(review => new ReviewResponseItem(
                            review.Id,
                            review.AlbumId,
                            review.UserId,
                            review.Rating,
                            review.Text,
                            review.CreatedAt,
                            review.UpdatedAt))
                        .ToArray())),

            _ => throw new InvalidOperationException(
                $"Unexpected list reviews by album status: {result.Status}.")
        };
    }

    /// <summary>
    /// Gets a specific review from an album.
    /// </summary>
    [HttpGet("{reviewId:guid}")]
    [ProducesResponseType(typeof(ReviewResponseItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid albumId,
        Guid reviewId,
        CancellationToken cancellationToken)
    {
        var result = await _getReviewByIdHandler.HandleAsync(
            new GetReviewByIdQuery(albumId, reviewId),
            cancellationToken);

        return result.Status switch
        {
            GetReviewByIdStatus.ReviewNotFound => NotFound(),

            GetReviewByIdStatus.Found => Ok(
                new ReviewResponseItem(
                    result.Review!.Id,
                    result.Review.AlbumId,
                    result.Review.UserId,
                    result.Review.Rating,
                    result.Review.Text,
                    result.Review.CreatedAt,
                    result.Review.UpdatedAt)),

            _ => throw new InvalidOperationException(
                $"Unexpected get review by id status: {result.Status}.")
        };
    }

    /// <summary>
    /// Updates an existing album review.
    /// </summary>
    [Authorize]
    [HttpPut("{reviewId:guid}")]
    [ProducesResponseType(typeof(UpdateReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid albumId,
        Guid reviewId,
        UpdateReviewRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateReviewCommand(
            albumId,
            reviewId,
            GetAuthenticatedUserId(),
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

    /// <summary>
    /// Deletes an album review.
    /// </summary>
    [Authorize]
    [HttpDelete("{reviewId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid albumId,
        Guid reviewId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteReviewCommand(
            albumId,
            reviewId,
            GetAuthenticatedUserId());

        var result = await _deleteReviewHandler.HandleAsync(
            command,
            cancellationToken);

        return result.Status switch
        {
            DeleteReviewStatus.ReviewNotFound => NotFound(),

            DeleteReviewStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden),

            DeleteReviewStatus.Deleted => NoContent(),

            _ => throw new InvalidOperationException(
                $"Unexpected delete review status: {result.Status}.")
        };
    }

    private Guid GetAuthenticatedUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            throw new InvalidOperationException(
                "Authenticated user id claim is missing or invalid.");
        }

        return parsedUserId;
    }
}