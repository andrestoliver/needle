using System.Net;
using System.Net.Http.Json;
using Needle.Application.Albums.SearchExternalAlbums;

namespace Needle.Infrastructure.ExternalCatalog.MusicBrainz;

public sealed class MusicBrainzAlbumCatalog(
    HttpClient httpClient) : IExternalAlbumCatalog
{
    public async Task<IReadOnlyCollection<ExternalAlbumSearchResult>>
        SearchAsync(
            string query,
            int limit,
            CancellationToken cancellationToken)
    {
        var musicBrainzQuery =
            $"{query} AND primarytype:album";

        var requestUri =
            $"/ws/2/release-group?query=" +
            $"{Uri.EscapeDataString(musicBrainzQuery)}" +
            $"&limit={limit}&fmt=json";

        var response = await httpClient.GetFromJsonAsync<
            MusicBrainzSearchResponse>(
            requestUri,
            cancellationToken);

        if (response is null)
        {
            return [];
        }

        return response.ReleaseGroups
            .Where(group =>
                string.Equals(
                    group.PrimaryType,
                    "Album",
                    StringComparison.OrdinalIgnoreCase))
            .Select(group => new ExternalAlbumSearchResult(
                group.Id,
                group.Title,
                group.ArtistCredit.FirstOrDefault()?.Name
                ?? "Unknown artist",
                ParseYear(group.FirstReleaseDate)))
            .ToArray();
    }

    private static int? ParseYear(string? releaseDate)
    {
        if (string.IsNullOrWhiteSpace(releaseDate) ||
            releaseDate.Length < 4)
        {
            return null;
        }

        return int.TryParse(releaseDate[..4], out var year)
            ? year
            : null;
    }
    
    public async Task<ExternalAlbumSearchResult?> GetByIdAsync(
        string externalId,
        CancellationToken cancellationToken)
    {
        var requestUri =
            $"/ws/2/release-group/{Uri.EscapeDataString(externalId)}" +
            "?inc=artist-credits&fmt=json";

        using var response = await httpClient.GetAsync(
            requestUri,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var releaseGroup = await response.Content
            .ReadFromJsonAsync<MusicBrainzReleaseGroupDto>(
                cancellationToken);

        if (releaseGroup is null ||
            !string.Equals(
                releaseGroup.PrimaryType,
                "Album",
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return new ExternalAlbumSearchResult(
            releaseGroup.Id,
            releaseGroup.Title,
            releaseGroup.ArtistCredit.FirstOrDefault()?.Name
            ?? "Unknown artist",
            ParseYear(releaseGroup.FirstReleaseDate));
    }
}