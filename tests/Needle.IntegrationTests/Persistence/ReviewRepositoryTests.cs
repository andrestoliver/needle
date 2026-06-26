using Microsoft.EntityFrameworkCore;
using Needle.Domain.Albums;
using Needle.Domain.Reviews;
using Needle.Infrastructure.Persistence;
using Needle.Infrastructure.Persistence.Repositories;
using Npgsql;

namespace Needle.IntegrationTests.Persistence;

public sealed class ReviewRepositoryTests
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=needle;Username=needle;Password=needle";

    [Fact]
    public async Task AddAsync_ShouldPersistReview()
    {
        var options = CreateOptions();

        await using var dbContext = new NeedleDbContext(options);
        await dbContext.Database.MigrateAsync();

        var repository = new ReviewRepository(dbContext);
        var album = await CreateAlbumAsync(dbContext);

        var review = Review.Create(
            Guid.NewGuid(),
            album.Id,
            Guid.NewGuid(),
            new Rating(4.5m),
            "great album",
            CreateUtcNow());

        try
        {
            await repository.AddAsync(review, CancellationToken.None);

            dbContext.ChangeTracker.Clear();

            var savedReview = await dbContext.Reviews
                .SingleOrDefaultAsync(item => item.Id == review.Id);

            Assert.NotNull(savedReview);
            Assert.Equal(review.Id, savedReview.Id);
            Assert.Equal(review.AlbumId, savedReview.AlbumId);
            Assert.Equal(review.UserId, savedReview.UserId);
            Assert.Equal(review.Rating, savedReview.Rating);
            Assert.Equal(review.Text, savedReview.Text);
            Assert.Equal(review.CreatedAt, savedReview.CreatedAt);
            Assert.Null(savedReview.UpdatedAt);
        }
        finally
        {
            await CleanReviewAsync(dbContext, review.Id);
            await CleanAlbumAsync(dbContext, album.Id);
        }
    }

    [Fact]
    public async Task GetByAlbumAndUserAsync_WhenReviewExists_ShouldReturnReview()
    {
        var options = CreateOptions();

        await using var dbContext = new NeedleDbContext(options);
        await dbContext.Database.MigrateAsync();

        var repository = new ReviewRepository(dbContext);
        var album = await CreateAlbumAsync(dbContext);
        var userId = Guid.NewGuid();

        var review = Review.Create(
            Guid.NewGuid(),
            album.Id,
            userId,
            new Rating(5.0m),
            "masterpiece",
            CreateUtcNow());

        try
        {
            dbContext.Reviews.Add(review);
            await dbContext.SaveChangesAsync();

            dbContext.ChangeTracker.Clear();

            var result = await repository.GetByAlbumAndUserAsync(
                album.Id,
                userId,
                CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(review.Id, result.Id);
            Assert.Equal(review.AlbumId, result.AlbumId);
            Assert.Equal(review.UserId, result.UserId);
            Assert.Equal(review.Rating, result.Rating);
            Assert.Equal(review.Text, result.Text);
            Assert.Equal(review.CreatedAt, result.CreatedAt);
        }
        finally
        {
            await CleanReviewAsync(dbContext, review.Id);
            await CleanAlbumAsync(dbContext, album.Id);
        }
    }

    [Fact]
    public async Task GetByAlbumAndUserAsync_WhenReviewDoesNotExist_ShouldReturnNull()
    {
        var options = CreateOptions();

        await using var dbContext = new NeedleDbContext(options);
        await dbContext.Database.MigrateAsync();

        var repository = new ReviewRepository(dbContext);
        var album = await CreateAlbumAsync(dbContext);

        try
        {
            var result = await repository.GetByAlbumAndUserAsync(
                album.Id,
                Guid.NewGuid(),
                CancellationToken.None);

            Assert.Null(result);
        }
        finally
        {
            await CleanAlbumAsync(dbContext, album.Id);
        }
    }

    [Fact]
    public async Task AddAsync_WhenAlbumAndUserAlreadyHaveReview_ShouldThrowPostgresException()
    {
        var options = CreateOptions();

        await using var dbContext = new NeedleDbContext(options);
        await dbContext.Database.MigrateAsync();

        var repository = new ReviewRepository(dbContext);
        var album = await CreateAlbumAsync(dbContext);
        var userId = Guid.NewGuid();

        var firstReview = Review.Create(
            Guid.NewGuid(),
            album.Id,
            userId,
            new Rating(4.0m),
            "first review",
            CreateUtcNow());

        var duplicatedReview = Review.Create(
            Guid.NewGuid(),
            album.Id,
            userId,
            new Rating(5.0m),
            "duplicated review",
            CreateUtcNow());

        try
        {
            await repository.AddAsync(firstReview, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<DbUpdateException>(
                () => repository.AddAsync(duplicatedReview, CancellationToken.None));

            var postgresException = Assert.IsType<PostgresException>(
                exception.InnerException);

            Assert.Equal(PostgresErrorCodes.UniqueViolation, postgresException.SqlState);
        }
        finally
        {
            await CleanReviewAsync(dbContext, firstReview.Id);
            await CleanReviewAsync(dbContext, duplicatedReview.Id);
            await CleanAlbumAsync(dbContext, album.Id);
        }
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistReviewChanges()
    {
        var options = CreateOptions();

        await using var dbContext = new NeedleDbContext(options);
        await dbContext.Database.MigrateAsync();

        var repository = new ReviewRepository(dbContext);
        var album = await CreateAlbumAsync(dbContext);

        var review = Review.Create(
            Guid.NewGuid(),
            album.Id,
            Guid.NewGuid(),
            new Rating(3.5m),
            "original text",
            CreateUtcNow());

        var updatedAt = CreateUtcNow().AddHours(1);

        try
        {
            dbContext.Reviews.Add(review);
            await dbContext.SaveChangesAsync();

            dbContext.ChangeTracker.Clear();

            var reviewToUpdate = await repository.GetByIdAsync(
                review.Id,
                CancellationToken.None);

            Assert.NotNull(reviewToUpdate);

            reviewToUpdate.Update(
                new Rating(5.0m),
                "updated text",
                updatedAt);

            await repository.UpdateAsync(
                reviewToUpdate,
                CancellationToken.None);

            dbContext.ChangeTracker.Clear();

            var updatedReview = await dbContext.Reviews
                .SingleAsync(item => item.Id == review.Id);

            Assert.Equal(review.Id, updatedReview.Id);
            Assert.Equal(review.AlbumId, updatedReview.AlbumId);
            Assert.Equal(review.UserId, updatedReview.UserId);
            Assert.Equal(new Rating(5.0m), updatedReview.Rating);
            Assert.Equal("updated text", updatedReview.Text);
            Assert.Equal(review.CreatedAt, updatedReview.CreatedAt);
            Assert.Equal(updatedAt, updatedReview.UpdatedAt);
        }
        finally
        {
            await CleanReviewAsync(dbContext, review.Id);
            await CleanAlbumAsync(dbContext, album.Id);
        }
    }
    
    private static DbContextOptions<NeedleDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<NeedleDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
    }

    private static async Task<Album> CreateAlbumAsync(
        NeedleDbContext dbContext)
    {
        var album = Album.CreateManual(
            Guid.NewGuid(),
            "Kind of Blue",
            "Miles Davis",
            1959);

        dbContext.Albums.Add(album);
        await dbContext.SaveChangesAsync();

        return album;
    }

    private static async Task CleanReviewAsync(
        NeedleDbContext dbContext,
        Guid reviewId)
    {
        await dbContext.Reviews
            .Where(review => review.Id == reviewId)
            .ExecuteDeleteAsync();
    }

    private static async Task CleanAlbumAsync(
        NeedleDbContext dbContext,
        Guid albumId)
    {
        await dbContext.Albums
            .Where(album => album.Id == albumId)
            .ExecuteDeleteAsync();
    }
    
    private static DateTimeOffset CreateUtcNow()
    {
        var utcNow = DateTimeOffset.UtcNow;

        return new DateTimeOffset(
            utcNow.Year,
            utcNow.Month,
            utcNow.Day,
            utcNow.Hour,
            utcNow.Minute,
            utcNow.Second,
            TimeSpan.Zero);
    }
    
    [Fact]
    public async Task ListByAlbumAsync_ShouldReturnReviewsFromAlbumOrderedByLatestActivity()
    {
        var options = CreateOptions();

        await using var dbContext = new NeedleDbContext(options);
        await dbContext.Database.MigrateAsync();

        var repository = new ReviewRepository(dbContext);
        var album = await CreateAlbumAsync(dbContext);
        var anotherAlbum = await CreateAlbumAsync(dbContext);

        var olderReview = Review.Create(
            Guid.NewGuid(),
            album.Id,
            Guid.NewGuid(),
            new Rating(3.5m),
            "older review",
            CreateUtcNow().AddHours(-2));

        var newerReview = Review.Create(
            Guid.NewGuid(),
            album.Id,
            Guid.NewGuid(),
            new Rating(4.5m),
            "newer review",
            CreateUtcNow().AddHours(-1));

        var reviewFromAnotherAlbum = Review.Create(
            Guid.NewGuid(),
            anotherAlbum.Id,
            Guid.NewGuid(),
            new Rating(5.0m),
            "another album review",
            CreateUtcNow());

        var updatedAt = CreateUtcNow().AddHours(1);

        olderReview.Update(
            new Rating(4.0m),
            "older review updated",
            updatedAt);

        try
        {
            dbContext.Reviews.AddRange(
                olderReview,
                newerReview,
                reviewFromAnotherAlbum);

            await dbContext.SaveChangesAsync();

            dbContext.ChangeTracker.Clear();

            var result = await repository.ListByAlbumAsync(
                album.Id,
                CancellationToken.None);

            Assert.Equal(2, result.Count);

            var reviews = result.ToArray();

            Assert.Equal(olderReview.Id, reviews[0].Id);
            Assert.Equal(updatedAt, reviews[0].UpdatedAt);
            Assert.Equal("older review updated", reviews[0].Text);

            Assert.Equal(newerReview.Id, reviews[1].Id);
            Assert.Null(reviews[1].UpdatedAt);

            Assert.All(
                reviews,
                review => Assert.Equal(album.Id, review.AlbumId));
        }
        finally
        {
            await CleanReviewAsync(dbContext, olderReview.Id);
            await CleanReviewAsync(dbContext, newerReview.Id);
            await CleanReviewAsync(dbContext, reviewFromAnotherAlbum.Id);
            await CleanAlbumAsync(dbContext, album.Id);
            await CleanAlbumAsync(dbContext, anotherAlbum.Id);
        }
    }
    
    [Fact]
    public async Task GetDetailsByIdAsync_WhenReviewExists_ShouldReturnReviewDetails()
    {
        var options = CreateOptions();

        await using var dbContext = new NeedleDbContext(options);
        await dbContext.Database.MigrateAsync();

        var repository = new ReviewRepository(dbContext);
        var album = await CreateAlbumAsync(dbContext);

        var review = Review.Create(
            Guid.NewGuid(),
            album.Id,
            Guid.NewGuid(),
            new Rating(4.5m),
            "great album",
            CreateUtcNow());

        try
        {
            dbContext.Reviews.Add(review);
            await dbContext.SaveChangesAsync();

            dbContext.ChangeTracker.Clear();

            var result = await repository.GetDetailsByIdAsync(
                review.Id,
                CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(review.Id, result.Id);
            Assert.Equal(review.AlbumId, result.AlbumId);
            Assert.Equal(review.UserId, result.UserId);
            Assert.Equal(review.Rating.Value, result.Rating);
            Assert.Equal(review.Text, result.Text);
            Assert.Equal(review.CreatedAt, result.CreatedAt);
            Assert.Equal(review.UpdatedAt, result.UpdatedAt);
        }
        finally
        {
            await CleanReviewAsync(dbContext, review.Id);
            await CleanAlbumAsync(dbContext, album.Id);
        }
    }
    
    [Fact]
    public async Task GetDetailsByIdAsync_WhenReviewDoesNotExist_ShouldReturnNull()
    {
        var options = CreateOptions();

        await using var dbContext = new NeedleDbContext(options);
        await dbContext.Database.MigrateAsync();

        var repository = new ReviewRepository(dbContext);

        var missingReviewId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        var result = await repository.GetDetailsByIdAsync(
            missingReviewId,
            CancellationToken.None);

        Assert.Null(result);
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldRemoveReview()
    {
        var options = new DbContextOptionsBuilder<NeedleDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=needle;Username=needle;Password=needle")
            .Options;

        await using var dbContext = new NeedleDbContext(options);

        await dbContext.Database.MigrateAsync();

        var repository = new ReviewRepository(dbContext);

        var album = Album.CreateManual(
            Guid.NewGuid(),
            "Kind of Blue",
            "Miles Davis",
            1959);

        var review = Review.Create(
            Guid.NewGuid(),
            album.Id,
            Guid.NewGuid(),
            new Rating(4.5m),
            "Review to delete.",
            new DateTimeOffset(2026, 6, 26, 12, 0, 0, TimeSpan.Zero));

        try
        {
            await dbContext.Albums.AddAsync(album);
            await dbContext.Reviews.AddAsync(review);
            await dbContext.SaveChangesAsync();

            dbContext.ChangeTracker.Clear();

            var persistedReview = await repository.GetByIdAsync(
                review.Id,
                CancellationToken.None);

            Assert.NotNull(persistedReview);

            await repository.DeleteAsync(
                persistedReview,
                CancellationToken.None);

            dbContext.ChangeTracker.Clear();

            var deletedReview = await dbContext.Reviews
                .AsNoTracking()
                .SingleOrDefaultAsync(storedReview => storedReview.Id == review.Id);

            Assert.Null(deletedReview);
        }
        finally
        {
            await dbContext.Reviews
                .Where(storedReview => storedReview.Id == review.Id)
                .ExecuteDeleteAsync();

            await dbContext.Albums
                .Where(storedAlbum => storedAlbum.Id == album.Id)
                .ExecuteDeleteAsync();
        }
    }
}