namespace Needle.Application.Reviews.GetReviewById;

public sealed record ReviewDetails(
    Guid Id,
    Guid AlbumId,
    Guid UserId,
    decimal Rating,
    string? Text,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);