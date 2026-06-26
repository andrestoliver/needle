using Needle.Application.Reviews;
using Needle.Application.Reviews.DeleteReview;
using Needle.Application.Reviews.GetReviewById;
using Needle.Application.Reviews.ListReviewsByAlbum;
using Needle.Domain.Reviews;

namespace Needle.UnitTests.Application.Reviews.DeleteReview;

public sealed class DeleteReviewHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenReviewExistsAndBelongsToUser_ShouldDeleteReview()
    {
        // Arrange
        var review = CreateReview();
        var repository = new FakeReviewRepository
        {
            Review = review
        };

        var handler = new DeleteReviewHandler(repository);

        var command = new DeleteReviewCommand(
            review.AlbumId,
            review.Id,
            review.UserId);

        // Act
        var result = await handler.HandleAsync(
            command,
            CancellationToken.None);

        // Assert
        Assert.Equal(DeleteReviewStatus.Deleted, result.Status);
        Assert.Same(review, repository.DeletedReview);
    }

    [Fact]
    public async Task HandleAsync_WhenReviewDoesNotExist_ShouldReturnReviewNotFound()
    {
        // Arrange
        var repository = new FakeReviewRepository();
        var handler = new DeleteReviewHandler(repository);

        var command = new DeleteReviewCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(
            command,
            CancellationToken.None);

        // Assert
        Assert.Equal(DeleteReviewStatus.ReviewNotFound, result.Status);
        Assert.Null(repository.DeletedReview);
    }

    [Fact]
    public async Task HandleAsync_WhenReviewBelongsToAnotherAlbum_ShouldReturnReviewNotFound()
    {
        // Arrange
        var review = CreateReview();
        var repository = new FakeReviewRepository
        {
            Review = review
        };

        var handler = new DeleteReviewHandler(repository);

        var command = new DeleteReviewCommand(
            Guid.NewGuid(),
            review.Id,
            review.UserId);

        // Act
        var result = await handler.HandleAsync(
            command,
            CancellationToken.None);

        // Assert
        Assert.Equal(DeleteReviewStatus.ReviewNotFound, result.Status);
        Assert.Null(repository.DeletedReview);
    }

    [Fact]
    public async Task HandleAsync_WhenReviewBelongsToAnotherUser_ShouldReturnForbidden()
    {
        // Arrange
        var review = CreateReview();
        var repository = new FakeReviewRepository
        {
            Review = review
        };

        var handler = new DeleteReviewHandler(repository);

        var command = new DeleteReviewCommand(
            review.AlbumId,
            review.Id,
            Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(
            command,
            CancellationToken.None);

        // Assert
        Assert.Equal(DeleteReviewStatus.Forbidden, result.Status);
        Assert.Null(repository.DeletedReview);
    }

    private static Review CreateReview()
    {
        return Review.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new Rating(4.5m),
            "Great album.",
            new DateTimeOffset(2026, 6, 26, 12, 0, 0, TimeSpan.Zero));
    }

    private sealed class FakeReviewRepository : IReviewRepository
    {
        public Review? Review { get; init; }
        public Review? DeletedReview { get; private set; }

        public Task AddAsync(
            Review review,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Review?> GetByAlbumAndUserAsync(
            Guid albumId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Review?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Review?.Id == id ? Review : null);
        }

        public Task UpdateAsync(
            Review review,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(
            Review review,
            CancellationToken cancellationToken)
        {
            DeletedReview = review;

            return Task.CompletedTask;
        }
        
        public Task<IReadOnlyCollection<ReviewListItem>> ListByAlbumAsync(
            Guid albumId,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ReviewDetails?> GetDetailsByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}