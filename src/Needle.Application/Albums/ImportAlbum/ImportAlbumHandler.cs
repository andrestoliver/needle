using Needle.Application.Albums.SearchExternalAlbums;
using Needle.Domain.Albums;

namespace Needle.Application.Albums.ImportAlbum;

public sealed class ImportAlbumHandler
{
    private readonly IAlbumRepository _albumRepository;
    private readonly IExternalAlbumCatalog _catalog;
    
    public ImportAlbumHandler(IAlbumRepository albumRepository, IExternalAlbumCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(albumRepository);
        ArgumentNullException.ThrowIfNull(catalog);
        
        _albumRepository = albumRepository;
        _catalog = catalog;
    }

    public async Task<ImportAlbumResult> HandleAsync(ImportAlbumCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var existingAlbum = await _albumRepository.GetByExternalIdAsync(command.ExternalId, cancellationToken);
        
        if (existingAlbum is not null)
        {
            return new ImportAlbumResult(ImportAlbumStatus.AlreadyImported, existingAlbum);
        }

        var externalAlbum = await _catalog.GetByIdAsync(
            command.ExternalId, 
            cancellationToken);
        
        
        if (externalAlbum is null)
        {
            return new ImportAlbumResult(ImportAlbumStatus.ExternalAlbumNotFound, null);
        }

        if (externalAlbum.FirstReleaseYear is null)
        {
            return new ImportAlbumResult(ImportAlbumStatus.MissingReleaseYear, null);
        }

        var albumToImport = Album.ImportFromMusicBrainz(
            Guid.NewGuid(),
            externalAlbum.ExternalId,
            externalAlbum.Title,
            externalAlbum.ArtistName,
            externalAlbum.FirstReleaseYear.Value);

        await _albumRepository.AddAsync(albumToImport, cancellationToken);
        
        return new ImportAlbumResult(ImportAlbumStatus.Imported, albumToImport);
    }
}