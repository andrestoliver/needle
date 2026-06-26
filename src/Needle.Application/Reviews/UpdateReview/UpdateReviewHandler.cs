using Needle.Application.Common.Time;
using Needle.Domain.Reviews;

namespace Needle.Application.Reviews.UpdateReview;

public sealed class UpdateReviewHandler
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IClock _clock;

    public UpdateReviewHandler(
        IReviewRepository reviewRepository,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(reviewRepository);
        ArgumentNullException.ThrowIfNull(clock);

        _reviewRepository = reviewRepository;
        _clock = clock;
    }

    public async Task<UpdateReviewResult> HandleAsync(
        UpdateReviewCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var review = await _reviewRepository.GetByIdAsync(
            command.ReviewId,
            cancellationToken);

        if (review is null)
        {
            return new UpdateReviewResult(
                UpdateReviewStatus.ReviewNotFound,
                null);
        }

        if (review.AlbumId != command.AlbumId)
        {
            return new UpdateReviewResult(
                UpdateReviewStatus.ReviewNotFound,
                null);
        }

        if (review.UserId != command.UserId)
        {
            return new UpdateReviewResult(
                UpdateReviewStatus.Forbidden,
                null);
        }

        review.Update(
            new Rating(command.Rating),
            command.Text,
            _clock.UtcNow);

        await _reviewRepository.UpdateAsync(
            review,
            cancellationToken);

        return new UpdateReviewResult(
            UpdateReviewStatus.Updated,
            review);
    }
}