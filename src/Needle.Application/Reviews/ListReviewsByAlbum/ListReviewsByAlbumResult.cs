namespace Needle.Application.Reviews.ListReviewsByAlbum;

public sealed record ListReviewsByAlbumResult(
    ListReviewsByAlbumStatus Status,
    IReadOnlyCollection<ReviewListItem> Reviews);