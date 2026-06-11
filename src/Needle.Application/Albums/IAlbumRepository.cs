using Needle.Domain.Albums;

namespace Needle.Application.Albums;

public interface IAlbumRepository
{
    Task AddAsync(Album album, CancellationToken cancellationToken);
}
