using Needle.Domain.Albums;

namespace Needle.Application.Albums.GetAlbumById;

public sealed class GetAlbumByIdHandler
{
    private readonly IAlbumRepository _albumRepository;

    public GetAlbumByIdHandler(IAlbumRepository albumRepository)
    {
        ArgumentNullException.ThrowIfNull(albumRepository);
        _albumRepository = albumRepository;
    }

    public async Task<Album?> HandleAsync(GetAlbumByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        return await _albumRepository.GetByIdAsync(query.Id, cancellationToken);
    }
}