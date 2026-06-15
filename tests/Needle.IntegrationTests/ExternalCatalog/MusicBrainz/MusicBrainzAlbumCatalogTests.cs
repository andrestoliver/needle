using System.Net;
using System.Text;
using Needle.Infrastructure.ExternalCatalog.MusicBrainz;

namespace Needle.IntegrationTests.ExternalCatalog.MusicBrainz;

public sealed class MusicBrainzAlbumCatalogTests
{
    [Fact]
    public async Task SearchAsync_ShouldMapAlbumResults()
    {
        const string json = """
        {
          "release-groups": [
            {
              "id": "album-id",
              "title": "Kind of Blue",
              "first-release-date": "1959-08-17",
              "primary-type": "Album",
              "artist-credit": [
                {
                  "name": "Miles Davis"
                }
              ]
            },
            {
              "id": "single-id",
              "title": "Some Single",
              "first-release-date": "1960",
              "primary-type": "Single",
              "artist-credit": [
                {
                  "name": "Some Artist"
                }
              ]
            },
            {
              "id": "unknown-date-id",
              "title": "Unknown Date",
              "first-release-date": "",
              "primary-type": "Album",
              "artist-credit": []
            }
          ]
        }
        """;

        var handler = new StubHttpMessageHandler(json);

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://musicbrainz.org")
        };

        var catalog = new MusicBrainzAlbumCatalog(httpClient);

        var results = await catalog.SearchAsync(
            "Kind of Blue",
            5,
            CancellationToken.None);

        Assert.Equal(2, results.Count);

        var album = Assert.Single(
            results,
            item => item.ExternalId == "album-id");

        Assert.Equal("Kind of Blue", album.Title);
        Assert.Equal("Miles Davis", album.ArtistName);
        Assert.Equal(1959, album.FirstReleaseYear);

        var unknownDateAlbum = Assert.Single(
            results,
            item => item.ExternalId == "unknown-date-id");

        Assert.Equal("Unknown artist", unknownDateAlbum.ArtistName);
        Assert.Null(unknownDateAlbum.FirstReleaseYear);
    }

    [Fact]
    public async Task SearchAsync_ShouldSendFilteredAndEncodedQuery()
    {
        var handler = new StubHttpMessageHandler(
            """{ "release-groups": [] }""");

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://musicbrainz.org")
        };

        var catalog = new MusicBrainzAlbumCatalog(httpClient);

        await catalog.SearchAsync(
            "A Love Supreme",
            7,
            CancellationToken.None);

        Assert.NotNull(handler.ReceivedRequestUri);

        var requestUri = handler.ReceivedRequestUri.ToString();

        var decodedQuery = Uri.UnescapeDataString(
            handler.ReceivedRequestUri.Query);

        Assert.Contains(
            "query=A Love Supreme AND primarytype:album",
            decodedQuery);

        Assert.Contains("limit=7", requestUri);
        Assert.Contains("fmt=json", requestUri);
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenAlbumExists_ShouldMapAlbum()
    {
        const string json = """
                            {
                              "id": "1b022e01-4da6-387b-8658-8678046e4cef",
                              "title": "Kind of Blue",
                              "first-release-date": "1959-08-17",
                              "primary-type": "Album",
                              "artist-credit": [
                                { "name": "Miles Davis" }
                              ]
                            }
                            """;

        var handler = new StubHttpMessageHandler(json);
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://musicbrainz.org")
        };

        var catalog = new MusicBrainzAlbumCatalog(client);

        var result = await catalog.GetByIdAsync(
            "1b022e01-4da6-387b-8658-8678046e4cef",
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Kind of Blue", result.Title);
        Assert.Equal("Miles Davis", result.ArtistName);
        Assert.Equal(1959, result.FirstReleaseYear);
    }

    private sealed class StubHttpMessageHandler(
        string responseContent,
        HttpStatusCode statusCode = HttpStatusCode.OK)
        : HttpMessageHandler
    {
        public Uri? ReceivedRequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            ReceivedRequestUri = request.RequestUri;

            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(
                    responseContent,
                    Encoding.UTF8,
                    "application/json")
            };

            return Task.FromResult(response);
        }
    }
}