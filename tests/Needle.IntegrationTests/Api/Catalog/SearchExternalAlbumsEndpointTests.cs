using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Needle.Application.Albums.SearchExternalAlbums;

namespace Needle.IntegrationTests.Api.Catalog;

public sealed class SearchExternalAlbumsEndpointTests
{
    [Fact]
    public async Task Get_WithValidQuery_ShouldReturnCatalogResults()
    {
        var expectedResults = new[]
        {
            new ExternalAlbumSearchResult(
                "external-id",
                "Kind of Blue",
                "Miles Davis",
                1959)
        };

        await using var factory =
            new CatalogWebApplicationFactory(expectedResults);

        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/catalog/albums?query=Kind%20of%20Blue&limit=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseBody = await response.Content
            .ReadFromJsonAsync<SearchExternalAlbumResponse[]>();

        Assert.NotNull(responseBody);

        var album = Assert.Single(responseBody);

        Assert.Equal("external-id", album.ExternalId);
        Assert.Equal("Kind of Blue", album.Title);
        Assert.Equal("Miles Davis", album.ArtistName);
        Assert.Equal(1959, album.FirstReleaseYear);
    }

    [Fact]
    public async Task Get_WhenCatalogReturnsNoResults_ShouldReturnEmptyArray()
    {
        await using var factory =
            new CatalogWebApplicationFactory([]);

        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/catalog/albums?query=unknown&limit=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseBody = await response.Content
            .ReadFromJsonAsync<SearchExternalAlbumResponse[]>();

        Assert.NotNull(responseBody);
        Assert.Empty(responseBody);
    }

    private sealed class CatalogWebApplicationFactory(
        IReadOnlyCollection<ExternalAlbumSearchResult> results)
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(
            IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IExternalAlbumCatalog>();

                services.AddSingleton<IExternalAlbumCatalog>(
                    new FakeExternalAlbumCatalog(results));
            });
        }
    }

    private sealed class FakeExternalAlbumCatalog(
        IReadOnlyCollection<ExternalAlbumSearchResult> results)
        : IExternalAlbumCatalog
    {
        public Task<IReadOnlyCollection<ExternalAlbumSearchResult>>
            SearchAsync(
                string query,
                int limit,
                CancellationToken cancellationToken)
        {
            return Task.FromResult(results);
        }
    }

    private sealed record SearchExternalAlbumResponse(
        string ExternalId,
        string Title,
        string ArtistName,
        int? FirstReleaseYear);
}