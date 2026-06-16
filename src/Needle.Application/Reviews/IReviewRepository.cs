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
}