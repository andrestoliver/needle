namespace Needle.Api.Contracts.Albums;

public sealed record CreateAlbumRequest(
    string Title,
    string ArtistName,
    int ReleaseYear);