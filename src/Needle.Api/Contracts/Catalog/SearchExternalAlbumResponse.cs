namespace Needle.Api.Contracts.Catalog;

public sealed record SearchExternalAlbumResponse(
    string ExternalId,
    string Title,
    string ArtistName,
    int? FirstReleaseYear);