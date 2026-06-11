using Microsoft.AspNetCore.Mvc;
using Needle.Api.Contracts.Albums;
using Needle.Application.Albums.CreateAlbum;
using Needle.Application.Albums.GetAlbumById;

namespace Needle.Api.Controllers;

[ApiController]
[Route("api/albums")]
public class AlbumsController : ControllerBase
{
    private readonly CreateAlbumHandler _createAlbumHandler;
    private readonly GetAlbumByIdHandler _getAlbumByIdHandler;

    public AlbumsController(
        CreateAlbumHandler createAlbumHandler,
        GetAlbumByIdHandler getAlbumByIdHandler)
    {
        ArgumentNullException.ThrowIfNull(createAlbumHandler);
        ArgumentNullException.ThrowIfNull(getAlbumByIdHandler);
        
        _createAlbumHandler = createAlbumHandler;
        _getAlbumByIdHandler = getAlbumByIdHandler;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateAlbumRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateAlbumCommand(
            request.Title,
            request.ArtistName,
            request.ReleaseYear);

        var album = await _createAlbumHandler.HandleAsync(command, cancellationToken);
        
        var response = new CreateAlbumResponse(
            album.Id,
            album.Title,
            album.ArtistName,
            album.ReleaseYear);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetAlbumByIdQuery(id);

        var album = await _getAlbumByIdHandler.HandleAsync(
            query,
            cancellationToken);

        if (album is null)
        {
            return NotFound();
        }

        var response = new GetAlbumByIdResponse(
            album.Id,
            album.Title,
            album.ArtistName,
            album.ReleaseYear);

        return Ok(response);
    }
}