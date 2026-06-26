namespace Needle.Api.Contracts.Reviews;

public sealed record CreateReviewResponse(
    Guid Id,
    Guid AlbumId,
    Guid UserId,
    decimal Rating,
    string? Text,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);