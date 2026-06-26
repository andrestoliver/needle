using Needle.Application.Albums;
using Needle.Application.Common.Time;
using Needle.Application.Reviews;
using Needle.Application.Reviews.CreateReview;
using Needle.Domain.Albums;
using Needle.Domain.Reviews;

namespace Needle.UnitTests.Application.Reviews.CreateReview;

public sealed class CreateReviewHandlerTests
{
    private static readonly DateTimeOffset UtcNow =
        new(2026, 6, 16, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task HandleAsync_WhenAlbumDoesNotExist_ShouldReturnAlbumNotFound()
    {
        var albumRepository = new FakeAlbumRepository();
        var reviewRepository = new FakeReviewRepository();
        var clock = new FakeClock(UtcNow);
        var handler = new CreateReviewHandler(
            albumRepository,
            reviewRepository,
            clock);

        var command = CreateCommand();

        var result = await handler.HandleAsync(
            command,
            CancellationToken.None);

        Assert.Equal(CreateReviewStatus.AlbumNotFound, result.Status);
        Assert.Null(result.Review);
        Assert.Null(reviewRepository.AddedReview);
    }

    [Fact]
    public async Task HandleAsync_WhenReviewAlreadyExists_ShouldReturnAlreadyReviewed()
    {
        var album = CreateAlbum();
        var userId = Guid.NewGuid();

        var existingReview = Review.Create(
            Guid.NewGuid(),
            album.Id,
            userId,
            new Rating(4.0m),
            "already reviewed",
            UtcNow);

        var albumRepository = new FakeAlbumRepository
        {
            Album = album
        };

        var reviewRepository = new FakeReviewRepository
        {
            ExistingReview = existingReview
        };

        var clock = new FakeClock(UtcNow);
        var handler = new CreateReviewHandler(
            albumRepository,
            reviewRepository,
            clock);

        var command = new CreateReviewCommand(
            album.Id,
            userId,
            5.0m,
            "new text");

        var result = await handler.HandleAsync(
            command,
            CancellationToken.None);

        Assert.Equal(CreateReviewStatus.AlreadyReviewed, result.Status);
        Assert.Null(result.Review);
        Assert.Null(reviewRepository.AddedReview);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateAndPersistReview()
    {
        var album = CreateAlbum();
        var userId = Guid.NewGuid();

        var albumRepository = new FakeAlbumRepository
        {
            Album = album
        };

        var reviewRepository = new FakeReviewRepository();
        var clock = new FakeClock(UtcNow);
        var handler = new CreateReviewHandler(
            albumRepository,
            reviewRepository,
            clock);

        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var command = new CreateReviewCommand(
            album.Id,
            userId,
            4.5m,
            "  beautiful record  ");

        var result = await handler.HandleAsync(
            command,
            cancellationToken);

        Assert.Equal(CreateReviewStatus.Created, result.Status);
        Assert.NotNull(result.Review);
        Assert.Same(result.Review, reviewRepository.AddedReview);

        Assert.NotEqual(Guid.Empty, result.Review.Id);
        Assert.Equal(album.Id, result.Review.AlbumId);
        Assert.Equal(userId, result.Review.UserId);
        Assert.Equal(new Rating(4.5m), result.Review.Rating);
        Assert.Equal("beautiful record", result.Review.Text);
        Assert.Equal(UtcNow, result.Review.CreatedAt);
        Assert.Null(result.Review.UpdatedAt);

        Assert.Equal(cancellationToken, albumRepository.ReceivedCancellationToken);
        Assert.Equal(cancellationToken, reviewRepository.ReceivedGetCancellationToken);
        Assert.Equal(cancellationToken, reviewRepository.ReceivedAddCancellationToken);
    }

    [Fact]
    public async Task HandleAsync_WithNullCommand_ShouldThrow()
    {
        var handler = new CreateReviewHandler(
            new FakeAlbumRepository(),
            new FakeReviewRepository(),
            new FakeClock(UtcNow));

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => handler.HandleAsync(null!, CancellationToken.None));

        Assert.Equal("command", exception.ParamName);
    }

    private static CreateReviewCommand CreateCommand()
    {
        return new CreateReviewCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            4.5m,
            "great album");
    }

    private static Album CreateAlbum()
    {
        return Album.CreateManual(
            Guid.NewGuid(),
            "Kind of Blue",
            "Miles Davis",
            1959);
    }

    private sealed class FakeAlbumRepository : IAlbumRepository
    {
        public Album? Album { get; init; }
        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task AddAsync(
            Album album,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<Album?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            ReceivedCancellationToken = cancellationToken;
            return Task.FromResult(Album?.Id == id ? Album : null);
        }

        public Task<Album?> GetByExternalIdAsync(
            string externalId,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeReviewRepository : IReviewRepository
    {
        public Review? ExistingReview { get; init; }
        public Review? AddedReview { get; private set; }
        public CancellationToken ReceivedGetCancellationToken { get; private set; }
        public CancellationToken ReceivedAddCancellationToken { get; private set; }

        public Task AddAsync(
            Review review,
            CancellationToken cancellationToken)
        {
            AddedReview = review;
            ReceivedAddCancellationToken = cancellationToken;

            return Task.CompletedTask;
        }

        public Task<Review?> GetByAlbumAndUserAsync(
            Guid albumId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            ReceivedGetCancellationToken = cancellationToken;

            if (ExistingReview is null)
            {
                return Task.FromResult<Review?>(null);
            }

            var isSameAlbumAndUser =
                ExistingReview.AlbumId == albumId &&
                ExistingReview.UserId == userId;

            return Task.FromResult(isSameAlbumAndUser ? ExistingReview : null);
        }
        
        public Task<Review?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task UpdateAsync(
            Review review,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}