namespace Needle.Application.Reviews.DeleteReview;

public sealed class DeleteReviewHandler
{
    private readonly IReviewRepository _reviewRepository;

    public DeleteReviewHandler(IReviewRepository reviewRepository)
    {
        ArgumentNullException.ThrowIfNull(reviewRepository);

        _reviewRepository = reviewRepository;
    }

    public async Task<DeleteReviewResult> HandleAsync(
        DeleteReviewCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var review = await _reviewRepository.GetByIdAsync(
            command.ReviewId,
            cancellationToken);

        if (review is null || review.AlbumId != command.AlbumId)
        {
            return new DeleteReviewResult(DeleteReviewStatus.ReviewNotFound);
        }

        if (review.UserId != command.UserId)
        {
            return new DeleteReviewResult(DeleteReviewStatus.Forbidden);
        }

        await _reviewRepository.DeleteAsync(
            review,
            cancellationToken);

        return new DeleteReviewResult(DeleteReviewStatus.Deleted);
    }
}