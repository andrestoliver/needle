using Needle.Domain.Albums;

namespace Needle.Application.Albums.CreateAlbum;

public sealed class CreateAlbumHandler
{
    private readonly IAlbumRepository _albumRepository;

    public CreateAlbumHandler(IAlbumRepository albumRepository)
    {
        ArgumentNullException.ThrowIfNull(albumRepository);
        _albumRepository = albumRepository;
    }

    public async Task<Album> HandleAsync(
        CreateAlbumCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        
        var album = new Album(
            Guid.NewGuid(),
            command.Title,
            command.ArtistName,
            command.ReleaseYear);
        
        await _albumRepository.AddAsync(album, cancellationToken);
        
        return album;
    }
}
