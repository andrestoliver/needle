using Needle.Domain.Albums;

namespace Needle.Application.Albums;

public interface IAlbumRepository
{
    Task AddAsync(Album album, CancellationToken cancellationToken);
    Task<Album?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Album?> GetByExternalIdAsync(
        string externalId,
        CancellationToken cancellationToken);
}
