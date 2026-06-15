namespace Needle.Api.Contracts.Albums;

public sealed record ImportAlbumResponse(
    Guid Id,
    string ExternalId,
    string Title,
    string ArtistName,
    int ReleaseYear);