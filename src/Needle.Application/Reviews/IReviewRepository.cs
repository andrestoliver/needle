using Needle.Application.Reviews.GetReviewById;
using Needle.Application.Reviews.ListReviewsByAlbum;
using Needle.Domain.Reviews;

namespace Needle.Application.Reviews;

public interface IReviewRepository
{
    Task AddAsync(
        Review review,
        CancellationToken cancellationToken);

    Task<Review?> GetByAlbumAndUserAsync(
        Guid albumId,
        Guid userId,
        CancellationToken cancellationToken);
    
    Task<Review?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        Review review,
        CancellationToken cancellationToken);
    
    Task<IReadOnlyCollection<ReviewListItem>> ListByAlbumAsync(
        Guid albumId,
        CancellationToken cancellationToken);
    
    Task<ReviewDetails?> GetDetailsByIdAsync(
        Guid id,
        CancellationToken cancellationToken);
    
    Task DeleteAsync(Review review, CancellationToken cancellationToken);
}