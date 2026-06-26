namespace Needle.Application.Reviews.ListReviewsByAlbum;

public sealed record ReviewListItem(
    Guid Id,
    Guid AlbumId,
    Guid UserId,
    decimal Rating,
    string? Text,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);