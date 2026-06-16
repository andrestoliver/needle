using Needle.Application.Albums;
using Needle.Application.Common.Time;
using Needle.Domain.Reviews;

namespace Needle.Application.Reviews.CreateReview;

public sealed class CreateReviewHandler
{
    private readonly IAlbumRepository _albumRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IClock _clock;

    public CreateReviewHandler(
        IAlbumRepository albumRepository,
        IReviewRepository reviewRepository,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(albumRepository);
        ArgumentNullException.ThrowIfNull(reviewRepository);
        ArgumentNullException.ThrowIfNull(clock);

        _albumRepository = albumRepository;
        _reviewRepository = reviewRepository;
        _clock = clock;
    }

    public async Task<CreateReviewResult> HandleAsync(
        CreateReviewCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var album = await _albumRepository.GetByIdAsync(
            command.AlbumId,
            cancellationToken);

        if (album is null)
        {
            return new CreateReviewResult(
                CreateReviewStatus.AlbumNotFound,
                null);
        }

        var existingReview = await _reviewRepository.GetByAlbumAndUserAsync(
            command.AlbumId,
            command.UserId,
            cancellationToken);

        if (existingReview is not null)
        {
            return new CreateReviewResult(
                CreateReviewStatus.AlreadyReviewed,
                null);
        }

        var review = Review.Create(
            Guid.NewGuid(),
            command.AlbumId,
            command.UserId,
            new Rating(command.Rating),
            command.Text,
            _clock.UtcNow);

        await _reviewRepository.AddAsync(
            review,
            cancellationToken);

        return new CreateReviewResult(
            CreateReviewStatus.Created,
            review);
    }
}