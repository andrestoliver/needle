namespace Needle.Application.Albums.SearchExternalAlbums;

public sealed record ExternalAlbumSearchResult(
    string ExternalId,
    string Title,
    string ArtistName,
    int? FirstReleaseYear);