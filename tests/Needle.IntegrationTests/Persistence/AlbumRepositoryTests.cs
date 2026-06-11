using Microsoft.EntityFrameworkCore;
using Needle.Domain.Albums;
using Needle.Infrastructure.Persistence;
using Needle.Infrastructure.Persistence.Repositories;

namespace Needle.IntegrationTests.Persistence;

public sealed class AlbumRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldPersistAlbum()
    {
        var options = new DbContextOptionsBuilder<NeedleDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=needle;Username=needle;Password=needle")
            .Options;

        await using var dbContext = new NeedleDbContext(options);
        
        await dbContext.Database.MigrateAsync();

        var repository = new AlbumRepository(dbContext);
        var album = new Album(
            Guid.NewGuid(),
            "Kind of Blue",
            "Miles Davis",
            1959);

        try
        {
            await repository.AddAsync(album, CancellationToken.None);

            dbContext.ChangeTracker.Clear();

            var persistedAlbum = await dbContext.Albums
                .SingleAsync(item => item.Id == album.Id);

            Assert.Equal(album.Id, persistedAlbum.Id);
            Assert.Equal(album.Title, persistedAlbum.Title);
            Assert.Equal(album.ArtistName, persistedAlbum.ArtistName);
            Assert.Equal(album.ReleaseYear, persistedAlbum.ReleaseYear);
        }
        finally
        {
            await dbContext.Albums
                .Where(item => item.Id == album.Id)
                .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task GetByIdAsync_WhenAlbumExists_ShouldReturnAlbum()
    {
        var options = new DbContextOptionsBuilder<NeedleDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=needle;Username=needle;Password=needle")
            .Options;
        
        await using var dbContext = new NeedleDbContext(options);
        
        await dbContext.Database.MigrateAsync();
        
        var repository = new AlbumRepository(dbContext);
        var album = new Album(
            Guid.NewGuid(),
            "Kind of Blue",
            "Miles Davis",
            1959);

        try
        {
            await dbContext.Albums.AddAsync(album);
            await dbContext.SaveChangesAsync();

            dbContext.ChangeTracker.Clear();

            var result = await repository.GetByIdAsync(
                album.Id,
                CancellationToken.None);
        
            Assert.NotNull(result);
            Assert.Equal(album.Id, result.Id);
            Assert.Equal(album.Title, result.Title);
            Assert.Equal(album.ArtistName, result.ArtistName);
            Assert.Equal(album.ReleaseYear, result.ReleaseYear);
        }
        finally
        {
            await dbContext.Albums
                .Where(item => item.Id == album.Id)
                .ExecuteDeleteAsync();
        }
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenAlbumDoesNotExist_ShouldReturnNull()
    {
        var options = new DbContextOptionsBuilder<NeedleDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=needle;Username=needle;Password=needle")
            .Options;

        await using var dbContext = new NeedleDbContext(options);

        await dbContext.Database.MigrateAsync();

        var repository = new AlbumRepository(dbContext);

        var result = await repository.GetByIdAsync(
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.Null(result);
    }
}