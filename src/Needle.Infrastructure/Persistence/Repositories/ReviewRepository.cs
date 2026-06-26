using Microsoft.EntityFrameworkCore;
using Needle.Application.Reviews;
using Needle.Domain.Reviews;

namespace Needle.Infrastructure.Persistence.Repositories;

public sealed class ReviewRepository : IReviewRepository
{
    private readonly NeedleDbContext _dbContext;

    public ReviewRepository(NeedleDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Review review,
        CancellationToken cancellationToken)
    {
        await _dbContext.Reviews.AddAsync(review, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Review?> GetByAlbumAndUserAsync(
        Guid albumId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Reviews
            .AsNoTracking()
            .SingleOrDefaultAsync(
                review =>
                    review.AlbumId == albumId &&
                    review.UserId == userId,
                cancellationToken);
    }
    
    public async Task<Review?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Reviews
            .SingleOrDefaultAsync(
                review => review.Id == id,
                cancellationToken);
    }
    
    public async Task UpdateAsync(
        Review review,
        CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}