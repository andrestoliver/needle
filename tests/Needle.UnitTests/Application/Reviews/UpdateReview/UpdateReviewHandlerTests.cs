using Needle.Application.Common.Time;
using Needle.Application.Reviews;
using Needle.Application.Reviews.GetReviewById;
using Needle.Application.Reviews.ListReviewsByAlbum;
using Needle.Application.Reviews.UpdateReview;
using Needle.Domain.Reviews;

namespace Needle.UnitTests.Application.Reviews.UpdateReview;

public sealed class UpdateReviewHandlerTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 6, 16, 12, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset UtcNow =
        new(2026, 6, 16, 13, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task HandleAsync_WhenReviewDoesNotExist_ShouldReturnReviewNotFound()
    {
        var repository = new FakeReviewRepository();
        var clock = new FakeClock(UtcNow);
        var handler = new UpdateReviewHandler(repository, clock);

        var command = CreateCommand();

        var result = await handler.HandleAsync(
            command,
            CancellationToken.None);

        Assert.Equal(UpdateReviewStatus.ReviewNotFound, result.Status);
        Assert.Null(result.Review);
        Assert.Null(repository.UpdatedReview);
    }

    [Fact]
    public async Task HandleAsync_WhenReviewBelongsToAnotherAlbum_ShouldReturnReviewNotFound()
    {
        var review = CreateReview();

        var repository = new FakeReviewRepository
        {
            Review = review
        };

        var clock = new FakeClock(UtcNow);
        var handler = new UpdateReviewHandler(repository, clock);

        var command = new UpdateReviewCommand(
            Guid.NewGuid(),
            review.Id,
            review.UserId,
            5.0m,
            "updated text");

        var result = await handler.HandleAsync(
            command,
            CancellationToken.None);

        Assert.Equal(UpdateReviewStatus.ReviewNotFound, result.Status);
        Assert.Null(result.Review);
        Assert.Null(repository.UpdatedReview);
    }

    [Fact]
    public async Task HandleAsync_WhenReviewBelongsToAnotherUser_ShouldReturnForbidden()
    {
        var review = CreateReview();

        var repository = new FakeReviewRepository
        {
            Review = review
        };

        var clock = new FakeClock(UtcNow);
        var handler = new UpdateReviewHandler(repository, clock);

        var command = new UpdateReviewCommand(
            review.AlbumId,
            review.Id,
            Guid.NewGuid(),
            5.0m,
            "updated text");

        var result = await handler.HandleAsync(
            command,
            CancellationToken.None);

        Assert.Equal(UpdateReviewStatus.Forbidden, result.Status);
        Assert.Null(result.Review);
        Assert.Null(repository.UpdatedReview);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldUpdateAndPersistReview()
    {
        var review = CreateReview();

        var repository = new FakeReviewRepository
        {
            Review = review
        };

        var clock = new FakeClock(UtcNow);
        var handler = new UpdateReviewHandler(repository, clock);

        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var command = new UpdateReviewCommand(
            review.AlbumId,
            review.Id,
            review.UserId,
            5.0m,
            "  updated text  ");

        var result = await handler.HandleAsync(
            command,
            cancellationToken);

        Assert.Equal(UpdateReviewStatus.Updated, result.Status);
        Assert.Same(review, result.Review);
        Assert.Same(review, repository.UpdatedReview);

        Assert.Equal(new Rating(5.0m), review.Rating);
        Assert.Equal("updated text", review.Text);
        Assert.Equal(CreatedAt, review.CreatedAt);
        Assert.Equal(UtcNow, review.UpdatedAt);

        Assert.Equal(cancellationToken, repository.ReceivedGetCancellationToken);
        Assert.Equal(cancellationToken, repository.ReceivedUpdateCancellationToken);
    }

    [Fact]
    public async Task HandleAsync_WithNullCommand_ShouldThrow()
    {
        var handler = new UpdateReviewHandler(
            new FakeReviewRepository(),
            new FakeClock(UtcNow));

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => handler.HandleAsync(null!, CancellationToken.None));

        Assert.Equal("command", exception.ParamName);
    }

    private static UpdateReviewCommand CreateCommand()
    {
        return new UpdateReviewCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            4.5m,
            "updated text");
    }

    private static Review CreateReview()
    {
        return Review.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new Rating(4.0m),
            "original text",
            CreatedAt);
    }

    private sealed class FakeReviewRepository : IReviewRepository
    {
        public Review? Review { get; init; }
        public Review? UpdatedReview { get; private set; }
        public CancellationToken ReceivedGetCancellationToken { get; private set; }
        public CancellationToken ReceivedUpdateCancellationToken { get; private set; }

        public Task AddAsync(
            Review review,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<Review?> GetByAlbumAndUserAsync(
            Guid albumId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<Review?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            ReceivedGetCancellationToken = cancellationToken;

            return Task.FromResult(
                Review?.Id == id
                    ? Review
                    : null);
        }

        public Task UpdateAsync(
            Review review,
            CancellationToken cancellationToken)
        {
            UpdatedReview = review;
            ReceivedUpdateCancellationToken = cancellationToken;

            return Task.CompletedTask;
        }
        
        public Task<IReadOnlyCollection<ReviewListItem>> ListByAlbumAsync(
            Guid albumId,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
        
        public Task<ReviewDetails?> GetDetailsByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
        
        public Task DeleteAsync(
            Review review,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class FakeClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}