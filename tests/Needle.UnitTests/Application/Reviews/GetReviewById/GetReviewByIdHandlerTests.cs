using Needle.Application.Reviews;
using Needle.Application.Reviews.GetReviewById;
using Needle.Application.Reviews.ListReviewsByAlbum;
using Needle.Domain.Reviews;

namespace Needle.UnitTests.Application.Reviews.GetReviewById;

public sealed class GetReviewByIdHandlerTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 6, 16, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task HandleAsync_WhenReviewDoesNotExist_ShouldReturnReviewNotFound()
    {
        var repository = new FakeReviewRepository();
        var handler = new GetReviewByIdHandler(repository);

        var query = new GetReviewByIdQuery(
            Guid.NewGuid(),
            Guid.NewGuid());

        var result = await handler.HandleAsync(
            query,
            CancellationToken.None);

        Assert.Equal(GetReviewByIdStatus.ReviewNotFound, result.Status);
        Assert.Null(result.Review);
    }

    [Fact]
    public async Task HandleAsync_WhenReviewBelongsToAnotherAlbum_ShouldReturnReviewNotFound()
    {
        var review = CreateReviewDetails();

        var repository = new FakeReviewRepository
        {
            Review = review
        };

        var handler = new GetReviewByIdHandler(repository);

        var query = new GetReviewByIdQuery(
            Guid.NewGuid(),
            review.Id);

        var result = await handler.HandleAsync(
            query,
            CancellationToken.None);

        Assert.Equal(GetReviewByIdStatus.ReviewNotFound, result.Status);
        Assert.Null(result.Review);
    }

    [Fact]
    public async Task HandleAsync_WhenReviewExists_ShouldReturnReview()
    {
        var review = CreateReviewDetails();

        var repository = new FakeReviewRepository
        {
            Review = review
        };

        var handler = new GetReviewByIdHandler(repository);

        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var query = new GetReviewByIdQuery(
            review.AlbumId,
            review.Id);

        var result = await handler.HandleAsync(
            query,
            cancellationToken);

        Assert.Equal(GetReviewByIdStatus.Found, result.Status);
        Assert.Same(review, result.Review);
        Assert.Equal(review.Id, repository.ReceivedReviewId);
        Assert.Equal(cancellationToken, repository.ReceivedCancellationToken);
    }

    [Fact]
    public async Task HandleAsync_WithNullQuery_ShouldThrow()
    {
        var handler = new GetReviewByIdHandler(
            new FakeReviewRepository());

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => handler.HandleAsync(null!, CancellationToken.None));

        Assert.Equal("query", exception.ParamName);
    }

    private static ReviewDetails CreateReviewDetails()
    {
        return new ReviewDetails(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            4.5m,
            "great album",
            CreatedAt,
            null);
    }

    private sealed class FakeReviewRepository : IReviewRepository
    {
        public ReviewDetails? Review { get; init; }
        public Guid ReceivedReviewId { get; private set; }
        public CancellationToken ReceivedCancellationToken { get; private set; }

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
            throw new NotSupportedException();
        }

        public Task<ReviewDetails?> GetDetailsByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            ReceivedReviewId = id;
            ReceivedCancellationToken = cancellationToken;

            return Task.FromResult(
                Review?.Id == id
                    ? Review
                    : null);
        }
        
        public Task DeleteAsync(
            Review review,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}