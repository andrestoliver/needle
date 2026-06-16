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
}