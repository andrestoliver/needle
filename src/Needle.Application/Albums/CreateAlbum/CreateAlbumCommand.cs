namespace Needle.Application.Albums.CreateAlbum;

public sealed record CreateAlbumCommand(
    string Title,
    string ArtistName,
    int ReleaseYear);
