namespace Needle.Application.Reviews.CreateReview;

public sealed record CreateReviewCommand(
    Guid AlbumId,
    Guid UserId,
    decimal Rating,
    string? Text);