namespace Needle.Application.Albums.SearchExternalAlbums;

public interface IExternalAlbumCatalog
{
    Task<IReadOnlyCollection<ExternalAlbumSearchResult>> SearchAsync(
        string query,
        int limit,
        CancellationToken cancellationToken);
    Task<ExternalAlbumSearchResult?> GetByIdAsync(
        string externalId,
        CancellationToken cancellationToken);
}