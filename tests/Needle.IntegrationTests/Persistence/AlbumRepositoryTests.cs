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
        var album = Album.CreateManual(
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
        var album = Album.CreateManual(
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
    
    [Fact]
    public async Task AddAsync_WithExternalId_ShouldPersistExternalId()
    {
        var options = new DbContextOptionsBuilder<NeedleDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=needle;Username=needle;Password=needle")
            .Options;

        await using var dbContext = new NeedleDbContext(options);
        await dbContext.Database.MigrateAsync();

        var repository = new AlbumRepository(dbContext);
        var album = Album.ImportFromMusicBrainz(
            Guid.NewGuid(),
            Guid.NewGuid().ToString(),
            "Kind of Blue",
            "Miles Davis",
            1959);

        try
        {
            await repository.AddAsync(album, CancellationToken.None);

            dbContext.ChangeTracker.Clear();

            var persistedAlbum = await repository.GetByIdAsync(
                album.Id,
                CancellationToken.None);

            Assert.NotNull(persistedAlbum);
            Assert.Equal(album.ExternalId, persistedAlbum.ExternalId);
        }
        finally
        {
            await dbContext.Albums
                .Where(item => item.Id == album.Id)
                .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task AddAsync_WithDuplicateExternalId_ShouldThrow()
    {
        var options = new DbContextOptionsBuilder<NeedleDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=needle;Username=needle;Password=needle")
            .Options;

        await using var dbContext = new NeedleDbContext(options);
        await dbContext.Database.MigrateAsync();

        var repository = new AlbumRepository(dbContext);
        
        const string externalId = "1b022e01-4da6-387b-8658-8678046e4cef";

        var firstAlbum = Album.ImportFromMusicBrainz(
            Guid.NewGuid(),
            externalId,
            "Kind of Blue",
            "Miles Davis",
            1959);
        
        var secondAlbum = Album.ImportFromMusicBrainz(
            Guid.NewGuid(),
            externalId,
            "Kind of Blue",
            "Miles Davis",
            1959);

        try
        {
            await repository.AddAsync(firstAlbum, CancellationToken.None);
            
            Task Act() => repository.AddAsync(
                secondAlbum,
                CancellationToken.None);

            await Assert.ThrowsAsync<DbUpdateException>(Act);
        }
        finally
        {
            dbContext.ChangeTracker.Clear();
            
            await dbContext.Albums
                .Where(item => item.Id == firstAlbum.Id)
                .ExecuteDeleteAsync();
        }
    }
}