using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Needle.Infrastructure.Persistence;

namespace Needle.IntegrationTests.Api.Albums;

public sealed class CreateAlbumEndpointTests
{
    [Fact]
    public async Task Post_WithValidRequest_ShouldReturnCreated()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        await ApplyMigrationsAsync(factory.Services);

        var request = new
        {
            title = "In Rainbows",
            artistName = "Radiohead",
            releaseYear = 2007
        };
        
        CreateAlbumResponse? createdAlbum = null;

        try
        {
            var response = await client.PostAsJsonAsync(
                "/api/albums",
                request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            
            createdAlbum = await response.Content
                .ReadFromJsonAsync<CreateAlbumResponse>();

            Assert.NotNull(createdAlbum);
            Assert.NotEqual(Guid.Empty, createdAlbum.Id);
            Assert.Equal(request.title, createdAlbum.Title);
            Assert.Equal(request.artistName, createdAlbum.ArtistName);
            Assert.Equal(request.releaseYear, createdAlbum.ReleaseYear);
        }
        finally
        {
            if (createdAlbum is not null)
            {
                await DeleteAlbumAsync(factory.Services, createdAlbum.Id);
            }
        }
    }

    private static async Task ApplyMigrationsAsync(
        IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<NeedleDbContext>();

        await dbContext.Database.MigrateAsync();
    }
    
    private sealed record CreateAlbumResponse(
        Guid Id,
        string Title,
        string ArtistName,
        int ReleaseYear);
    
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
}