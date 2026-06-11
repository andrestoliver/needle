using Needle.Application.Albums;
using Needle.Domain.Albums;

namespace Needle.Infrastructure.Persistence.Repositories;

public sealed class AlbumRepository : IAlbumRepository
{
    private readonly NeedleDbContext _dbContext;

    public AlbumRepository(NeedleDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Album album,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(album);
        
        await _dbContext.Albums.AddAsync(album, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}