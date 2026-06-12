using Microsoft.AspNetCore.Mvc;
using Needle.Api.Contracts.Catalog;
using Needle.Application.Albums.SearchExternalAlbums;

namespace Needle.Api.Controllers;

[ApiController]
[Route("api/catalog/albums")]
public class CatalogController : ControllerBase
{
    private readonly SearchExternalAlbumsHandler _handler;

    public CatalogController(SearchExternalAlbumsHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        
        _handler = handler;
    }
    
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string query, 
        [FromQuery] int limit = 10, 
        CancellationToken cancellationToken = default)
    {
        var searchQuery = new SearchExternalAlbumsQuery(query, limit);
        
        var albums = await _handler.HandleAsync(
            searchQuery, 
            cancellationToken);

        var response = albums.Select(album => new ExternalAlbumSearchResult(
            album.ExternalId,
            album.Title,
            album.ArtistName,
            album.FirstReleaseYear ))
            .ToArray();
        
        return Ok(response);
    }
}