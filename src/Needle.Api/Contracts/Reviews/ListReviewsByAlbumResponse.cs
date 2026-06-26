namespace Needle.Api.Contracts.Reviews;

public sealed record ListReviewsByAlbumResponse(
    IReadOnlyCollection<ReviewResponseItem> Reviews);

public sealed record ReviewResponseItem(
    Guid Id,
    Guid AlbumId,
    Guid UserId,
    decimal Rating,
    string? Text,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);