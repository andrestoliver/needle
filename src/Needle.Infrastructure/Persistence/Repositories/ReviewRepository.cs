using Microsoft.EntityFrameworkCore;
using Needle.Application.Reviews;
using Needle.Application.Reviews.GetReviewById;
using Needle.Application.Reviews.ListReviewsByAlbum;
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
    
    public async Task<IReadOnlyCollection<ReviewListItem>> ListByAlbumAsync(
        Guid albumId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Reviews
            .AsNoTracking()
            .Where(review => review.AlbumId == albumId)
            .OrderByDescending(review => review.UpdatedAt ?? review.CreatedAt)
            .Select(review => new ReviewListItem(
                review.Id,
                review.AlbumId,
                review.UserId,
                review.Rating.Value,
                review.Text,
                review.CreatedAt,
                review.UpdatedAt))
            .ToArrayAsync(cancellationToken);
    }
    
    public async Task<ReviewDetails?> GetDetailsByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Reviews
            .AsNoTracking()
            .Where(review => review.Id == id)
            .Select(review => new ReviewDetails(
                review.Id,
                review.AlbumId,
                review.UserId,
                review.Rating.Value,
                review.Text,
                review.CreatedAt,
                review.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);
    }
    
    public async Task DeleteAsync(
        Review review,
        CancellationToken cancellationToken)
    {
        _dbContext.Reviews.Remove(review);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}