using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Needle.Domain.Albums;
using Needle.Infrastructure.Persistence;

namespace Needle.IntegrationTests.Api.Albums;

public sealed class GetAlbumByIdEndpointTests
{
    [Fact]
    public async Task Get_WhenAlbumExists_ShouldReturnOk()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        await ApplyMigrationsAsync(factory.Services);

        var album = new Album(
            Guid.NewGuid(),
            "A Love Supreme",
            "John Coltrane",
            1965);

        await AddAlbumAsync(factory.Services, album);

        try
        {
            var response = await client.GetAsync(
                $"/api/albums/{album.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseBody = await response.Content
                .ReadFromJsonAsync<GetAlbumByIdResponse>();

            Assert.NotNull(responseBody);
            Assert.Equal(album.Id, responseBody.Id);
            Assert.Equal(album.Title, responseBody.Title);
            Assert.Equal(album.ArtistName, responseBody.ArtistName);
            Assert.Equal(album.ReleaseYear, responseBody.ReleaseYear);
        }
        finally
        {
            await DeleteAlbumAsync(factory.Services, album.Id);
        }
    }

    [Fact]
    public async Task Get_WhenAlbumDoesNotExist_ShouldReturnNotFound()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        await ApplyMigrationsAsync(factory.Services);

        var nonExistingAlbumId = Guid.NewGuid();

        var response = await client.GetAsync(
            $"/api/albums/{nonExistingAlbumId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task ApplyMigrationsAsync(
        IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<NeedleDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    private static async Task AddAlbumAsync(
        IServiceProvider services,
        Album album)
    {
        await using var scope = services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<NeedleDbContext>();

        await dbContext.Albums.AddAsync(album);
        await dbContext.SaveChangesAsync();
    }

    private static async Task DeleteAlbumAsync(
        IServiceProvider services,
        Guid albumId)
    {
        await using var scope = services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<NeedleDbContext>();

        await dbContext.Albums
            .Where(album => album.Id == albumId)
            .ExecuteDeleteAsync();
    }

    private sealed record GetAlbumByIdResponse(
        Guid Id,
        string Title,
        string ArtistName,
        int ReleaseYear);
}