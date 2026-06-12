namespace Needle.Application.Albums.SearchExternalAlbums;

public sealed class SearchExternalAlbumsHandler
{
    private readonly IExternalAlbumCatalog _catalog;

    public SearchExternalAlbumsHandler(IExternalAlbumCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        _catalog = catalog;
    }

    public async Task<IReadOnlyCollection<ExternalAlbumSearchResult>> HandleAsync(
        SearchExternalAlbumsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        return await _catalog.SearchAsync(
            query.Query,
            query.Limit,
            cancellationToken);
    }
}
