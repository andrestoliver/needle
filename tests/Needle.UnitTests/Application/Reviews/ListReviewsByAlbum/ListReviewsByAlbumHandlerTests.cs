using Needle.Application.Albums;
using Needle.Application.Reviews;
using Needle.Application.Reviews.GetReviewById;
using Needle.Application.Reviews.ListReviewsByAlbum;
using Needle.Domain.Albums;
using Needle.Domain.Reviews;

namespace Needle.UnitTests.Application.Reviews.ListReviewsByAlbum;

public sealed class ListReviewsByAlbumHandlerTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 6, 16, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task HandleAsync_WhenAlbumDoesNotExist_ShouldReturnAlbumNotFound()
    {
        var albumRepository = new FakeAlbumRepository();
        var reviewRepository = new FakeReviewRepository();
        var handler = new ListReviewsByAlbumHandler(
            albumRepository,
            reviewRepository);

        var query = new ListReviewsByAlbumQuery(Guid.NewGuid());

        var result = await handler.HandleAsync(
            query,
            CancellationToken.None);

        Assert.Equal(ListReviewsByAlbumStatus.AlbumNotFound, result.Status);
        Assert.Empty(result.Reviews);
        Assert.False(reviewRepository.ListByAlbumWasCalled);
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumExists_ShouldReturnReviews()
    {
        var album = CreateAlbum();
        var reviews = new[]
        {
            new ReviewListItem(
                Guid.NewGuid(),
                album.Id,
                Guid.NewGuid(),
                4.5m,
                "great album",
                CreatedAt,
                null)
        };

        var albumRepository = new FakeAlbumRepository
        {
            Album = album
        };

        var reviewRepository = new FakeReviewRepository
        {
            Reviews = reviews
        };

        var handler = new ListReviewsByAlbumHandler(
            albumRepository,
            reviewRepository);

        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var query = new ListReviewsByAlbumQuery(album.Id);

        var result = await handler.HandleAsync(
            query,
            cancellationToken);

        Assert.Equal(ListReviewsByAlbumStatus.Found, result.Status);
        Assert.Equal(reviews, result.Reviews);
        Assert.Equal(album.Id, reviewRepository.ReceivedAlbumId);
        Assert.Equal(cancellationToken, albumRepository.ReceivedCancellationToken);
        Assert.Equal(cancellationToken, reviewRepository.ReceivedCancellationToken);
    }

    [Fact]
    public async Task HandleAsync_WithNullQuery_ShouldThrow()
    {
        var handler = new ListReviewsByAlbumHandler(
            new FakeAlbumRepository(),
            new FakeReviewRepository());

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => handler.HandleAsync(null!, CancellationToken.None));

        Assert.Equal("query", exception.ParamName);
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
        public IReadOnlyCollection<ReviewListItem> Reviews { get; init; } =
            [];

        public Guid ReceivedAlbumId { get; private set; }
        public CancellationToken ReceivedCancellationToken { get; private set; }
        public bool ListByAlbumWasCalled { get; private set; }

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
            throw new NotSupportedException();
        }

        public Task UpdateAsync(
            Review review,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyCollection<ReviewListItem>> ListByAlbumAsync(
            Guid albumId,
            CancellationToken cancellationToken)
        {
            ListByAlbumWasCalled = true;
            ReceivedAlbumId = albumId;
            ReceivedCancellationToken = cancellationToken;

            return Task.FromResult(Reviews);
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
}