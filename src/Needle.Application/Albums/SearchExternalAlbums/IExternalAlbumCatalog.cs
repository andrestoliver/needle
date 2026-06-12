namespace Needle.Application.Albums.SearchExternalAlbums;

public interface IExternalAlbumCatalog
{
    Task<IReadOnlyCollection<ExternalAlbumSearchResult>> SearchAsync(
        string query,
        int limit,
        CancellationToken cancellationToken);
}