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
}