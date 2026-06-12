using System.Text.Json.Serialization;

namespace Needle.Infrastructure.ExternalCatalog.MusicBrainz;

internal sealed record MusicBrainzSearchResponse(
    [property: JsonPropertyName("release-groups")]
    IReadOnlyCollection<MusicBrainzReleaseGroupDto> ReleaseGroups);

internal sealed record MusicBrainzReleaseGroupDto(
    [property: JsonPropertyName("id")]
    string Id,

    [property: JsonPropertyName("title")]
    string Title,

    [property: JsonPropertyName("first-release-date")]
    string? FirstReleaseDate,

    [property: JsonPropertyName("primary-type")]
    string? PrimaryType,

    [property: JsonPropertyName("artist-credit")]
    IReadOnlyCollection<MusicBrainzArtistCreditDto> ArtistCredit);

internal sealed record MusicBrainzArtistCreditDto(
    [property: JsonPropertyName("name")]
    string Name);