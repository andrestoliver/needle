using Needle.Domain.Reviews;

namespace Needle.UnitTests.Domain.Reviews;

public sealed class ReviewTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 6, 16, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidData_ShouldCreateReview()
    {
        var id = Guid.NewGuid();
        var albumId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var rating = new Rating(4.5m);

        var review = Review.Create(
            id,
            albumId,
            userId,
            rating,
            "great album",
            CreatedAt);

        Assert.Equal(id, review.Id);
        Assert.Equal(albumId, review.AlbumId);
        Assert.Equal(userId, review.UserId);
        Assert.Equal(rating, review.Rating);
        Assert.Equal("great album", review.Text);
        Assert.Equal(CreatedAt, review.CreatedAt);
        Assert.Null(review.UpdatedAt);
    }

    [Fact]
    public void Create_WithEmptyId_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => CreateReview(id: Guid.Empty));

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Create_WithEmptyAlbumId_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => CreateReview(albumId: Guid.Empty));

        Assert.Equal("albumId", exception.ParamName);
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => CreateReview(userId: Guid.Empty));

        Assert.Equal("userId", exception.ParamName);
    }

    [Fact]
    public void Create_WithDefaultRating_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Review.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                default,
                "great album",
                CreatedAt));

        Assert.Equal("rating", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyText_ShouldStoreNull(string? text)
    {
        var review = CreateReview(text: text);

        Assert.Null(review.Text);
    }

    [Fact]
    public void Create_WithText_ShouldTrimText()
    {
        var review = CreateReview(text: "  great album  ");

        Assert.Equal("great album", review.Text);
    }

    [Fact]
    public void Create_WithTextLongerThan2000Characters_ShouldThrow()
    {
        var text = new string('a', 2001);

        var exception = Assert.Throws<ArgumentException>(
            () => CreateReview(text: text));

        Assert.Equal("text", exception.ParamName);
    }

    [Fact]
    public void Create_WithDefaultCreatedAt_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Review.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                new Rating(4.5m),
                "great album",
                default));

        Assert.Equal("createdAt", exception.ParamName);
    }

    private static Review CreateReview(
        Guid? id = null,
        Guid? albumId = null,
        Guid? userId = null,
        Rating? rating = null,
        string? text = "great album",
        DateTimeOffset? createdAt = null)
    {
        return Review.Create(
            id ?? Guid.NewGuid(),
            albumId ?? Guid.NewGuid(),
            userId ?? Guid.NewGuid(),
            rating ?? new Rating(4.5m),
            text,
            createdAt ?? CreatedAt);
    }
}