namespace Needle.Application.Reviews.DeleteReview;

public sealed record DeleteReviewCommand
{
    public DeleteReviewCommand(
        Guid albumId,
        Guid reviewId,
        Guid userId)
    {
        if (albumId == Guid.Empty)
        {
            throw new ArgumentException("Album id cannot be empty.", nameof(albumId));
        }

        if (reviewId == Guid.Empty)
        {
            throw new ArgumentException("Review id cannot be empty.", nameof(reviewId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        AlbumId = albumId;
        ReviewId = reviewId;
        UserId = userId;
    }

    public Guid AlbumId { get; }
    public Guid ReviewId { get; }
    public Guid UserId { get; }
}