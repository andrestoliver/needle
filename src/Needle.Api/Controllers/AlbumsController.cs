using Microsoft.AspNetCore.Mvc;
using Needle.Api.Contracts.Albums;
using Needle.Application.Albums.CreateAlbum;
using Needle.Application.Albums.GetAlbumById;
using Needle.Application.Albums.ImportAlbum;

namespace Needle.Api.Controllers;

[ApiController]
[Route("api/albums")]
public class AlbumsController : ControllerBase
{
    private readonly CreateAlbumHandler _createAlbumHandler;
    private readonly GetAlbumByIdHandler _getAlbumByIdHandler;
    private readonly ImportAlbumHandler _importAlbumHandler;

    public AlbumsController(
        CreateAlbumHandler createAlbumHandler,
        GetAlbumByIdHandler getAlbumByIdHandler,
        ImportAlbumHandler importAlbumHandler)
    {
        ArgumentNullException.ThrowIfNull(createAlbumHandler);
        ArgumentNullException.ThrowIfNull(getAlbumByIdHandler);
        ArgumentNullException.ThrowIfNull(importAlbumHandler);
        
        _createAlbumHandler = createAlbumHandler;
        _getAlbumByIdHandler = getAlbumByIdHandler;
        _importAlbumHandler = importAlbumHandler;
    }
    
    /// <summary>
    /// Creates a local album manually.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateAlbumResponse), StatusCodes.Status201Created)]
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
    
    /// <summary>
    /// Gets a local album by id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetAlbumByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Imports an album from MusicBrainz into the local Needle catalog.
    /// </summary>
    [HttpPost("import")]
    [ProducesResponseType(typeof(ImportAlbumResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ImportAlbumResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Import(
        ImportAlbumRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ImportAlbumCommand(request.ExternalId);

        var result = await _importAlbumHandler.HandleAsync(
            command,
            cancellationToken);

        if (result.Album is not null)
        {
            var response = new ImportAlbumResponse(
                result.Album.Id,
                result.Album.ExternalId!,
                result.Album.Title,
                result.Album.ArtistName,
                result.Album.ReleaseYear);

            return result.Status switch
            {
                ImportAlbumStatus.Imported => CreatedAtAction(
                    nameof(GetById),
                    new { id = response.Id },
                    response),

                ImportAlbumStatus.AlreadyImported => Ok(response),

                _ => throw new InvalidOperationException(
                    "Unexpected import status with an album.")
            };
        }

        return result.Status switch
        {
            ImportAlbumStatus.ExternalAlbumNotFound => NotFound(
                new { message = "Album was not found in MusicBrainz." }),

            ImportAlbumStatus.MissingReleaseYear => UnprocessableEntity(
                new { message = "Album does not have a release year." }),

            _ => throw new InvalidOperationException(
                "Unexpected import status without an album.")
        };
    }
}