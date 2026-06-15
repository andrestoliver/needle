using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Needle.Application.Albums.SearchExternalAlbums;
using Needle.Infrastructure.Persistence;

namespace Needle.IntegrationTests.Api.Albums;

public sealed class ImportAlbumEndpointTests
{
    private const string ExternalId =
        "1b022e01-4da6-387b-8658-8678046e4cef";

    [Fact]
    public async Task Post_WhenAlbumIsNew_ShouldCreateAndBeIdempotent()
    {
        var externalAlbum = new ExternalAlbumSearchResult(
            ExternalId,
            "Kind of Blue",
            "Miles Davis",
            1959);

        await using var factory =
            new ImportAlbumWebApplicationFactory(externalAlbum);

        using var client = factory.CreateClient();

        await ApplyMigrationsAsync(factory.Services);
        await DeleteByExternalIdAsync(factory.Services, ExternalId);

        Guid? createdAlbumId = null;

        try
        {
            var request = new
            {
                externalId = ExternalId
            };

            var firstResponse = await client.PostAsJsonAsync(
                "/api/albums/import",
                request);

            Assert.Equal(
                HttpStatusCode.Created,
                firstResponse.StatusCode);

            var firstBody = await firstResponse.Content
                .ReadFromJsonAsync<ImportAlbumResponse>();

            Assert.NotNull(firstBody);

            createdAlbumId = firstBody.Id;

            Assert.Equal(ExternalId, firstBody.ExternalId);
            Assert.NotNull(firstResponse.Headers.Location);

            var secondResponse = await client.PostAsJsonAsync(
                "/api/albums/import",
                request);

            Assert.Equal(
                HttpStatusCode.OK,
                secondResponse.StatusCode);

            var secondBody = await secondResponse.Content
                .ReadFromJsonAsync<ImportAlbumResponse>();

            Assert.NotNull(secondBody);
            Assert.Equal(firstBody.Id, secondBody.Id);
        }
        finally
        {
            await DeleteByExternalIdAsync(
                factory.Services,
                ExternalId);
        }
    }
    
    [Fact]
    public async Task Post_WhenExternalAlbumDoesNotExist_ShouldReturnNotFound()
    {
        await using var factory =
            new ImportAlbumWebApplicationFactory(null);

        using var client = factory.CreateClient();

        await ApplyMigrationsAsync(factory.Services);
        await DeleteByExternalIdAsync(factory.Services, ExternalId);

        var response = await client.PostAsJsonAsync(
            "/api/albums/import",
            new { externalId = ExternalId });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenReleaseYearIsMissing_ShouldReturnUnprocessableEntity()
    {
        var externalAlbum = new ExternalAlbumSearchResult(
            ExternalId,
            "Kind of Blue",
            "Miles Davis",
            null);

        await using var factory =
            new ImportAlbumWebApplicationFactory(externalAlbum);

        using var client = factory.CreateClient();

        await ApplyMigrationsAsync(factory.Services);
        await DeleteByExternalIdAsync(factory.Services, ExternalId);

        var response = await client.PostAsJsonAsync(
            "/api/albums/import",
            new { externalId = ExternalId });

        Assert.Equal(
            HttpStatusCode.UnprocessableEntity,
            response.StatusCode);
    }

    private static async Task ApplyMigrationsAsync(
        IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<NeedleDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    private static async Task DeleteByExternalIdAsync(
        IServiceProvider services,
        string externalId)
    {
        await using var scope = services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<NeedleDbContext>();

        await dbContext.Albums
            .Where(album => album.ExternalId == externalId)
            .ExecuteDeleteAsync();
    }

    private sealed class ImportAlbumWebApplicationFactory(
        ExternalAlbumSearchResult? externalAlbum)
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(
            IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IExternalAlbumCatalog>();

                services.AddSingleton<IExternalAlbumCatalog>(
                    new FakeExternalAlbumCatalog(externalAlbum));
            });
        }
    }

    private sealed class FakeExternalAlbumCatalog(
        ExternalAlbumSearchResult? externalAlbum)
        : IExternalAlbumCatalog
    {
        public Task<ExternalAlbumSearchResult?> GetByIdAsync(
            string externalId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(externalAlbum);
        }

        public Task<IReadOnlyCollection<ExternalAlbumSearchResult>>
            SearchAsync(
                string query,
                int limit,
                CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }

    private sealed record ImportAlbumResponse(
        Guid Id,
        string ExternalId,
        string Title,
        string ArtistName,
        int ReleaseYear);
}