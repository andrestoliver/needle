namespace Needle.Application.Reviews.UpdateReview;

public sealed record UpdateReviewCommand(
    Guid AlbumId,
    Guid ReviewId,
    Guid UserId,
    decimal Rating,
    string? Text);