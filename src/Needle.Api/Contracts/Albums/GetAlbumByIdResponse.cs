namespace Needle.Api.Contracts.Albums;

public sealed record GetAlbumByIdResponse(
    Guid Id,
    string Title,
    string ArtistName,
    int ReleaseYear);