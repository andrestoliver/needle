namespace Needle.Api.Contracts.Albums;

public sealed record CreateAlbumResponse(
    Guid Id,
    string Title,
    string ArtistName,
    int ReleaseYear);