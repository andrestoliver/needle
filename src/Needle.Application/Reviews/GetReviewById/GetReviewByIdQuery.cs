namespace Needle.Application.Reviews.GetReviewById;

public sealed record GetReviewByIdQuery(
    Guid AlbumId,
    Guid ReviewId);