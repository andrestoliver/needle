using Microsoft.AspNetCore.Mvc;
using Needle.Api.Contracts.Albums;
using Needle.Application.Albums.CreateAlbum;

namespace Needle.Api.Controllers;

[ApiController]
[Route("api/albums")]
public class AlbumsController : ControllerBase
{
    private readonly CreateAlbumHandler _handler;

    public AlbumsController(CreateAlbumHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _handler = handler;
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

        var album = await _handler.HandleAsync(command, cancellationToken);
        
        var response = new CreateAlbumResponse(
            album.Id,
            album.Title,
            album.ArtistName,
            album.ReleaseYear);

        return StatusCode(StatusCodes.Status201Created, response);
    }
}