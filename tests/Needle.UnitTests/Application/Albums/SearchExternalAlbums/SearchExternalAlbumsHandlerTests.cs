using Needle.Application.Albums.SearchExternalAlbums;

namespace Needle.UnitTests.Application.Albums.SearchExternalAlbums;

public sealed class SearchExternalAlbumsHandlerTests
{
    [Fact]
    public void Query_WithSurroundingSpaces_ShouldTrimQuery()
    {
        var query = new SearchExternalAlbumsQuery(
            "  Kind of Blue  ");

        Assert.Equal("Kind of Blue", query.Query);
        Assert.Equal(10, query.Limit);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Query_WithEmptyText_ShouldThrow(string text)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new SearchExternalAlbumsQuery(text));

        Assert.Equal("query", exception.ParamName);
    }

    [Fact]
    public void Query_WithNullText_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new SearchExternalAlbumsQuery(null!));

        Assert.Equal("query", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    public void Query_WithLimitOutsideRange_ShouldThrow(int limit)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new SearchExternalAlbumsQuery("Kind of Blue", limit));

        Assert.Equal("limit", exception.ParamName);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(20)]
    public void Query_WithBoundaryLimit_ShouldAccept(int limit)
    {
        var query = new SearchExternalAlbumsQuery(
            "Kind of Blue",
            limit);

        Assert.Equal(limit, query.Limit);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnCatalogResultsAndPropagateArguments()
    {
        var expectedResults = new[]
        {
            new ExternalAlbumSearchResult(
                "external-id",
                "Kind of Blue",
                "Miles Davis",
                1959)
        };

        var catalog = new FakeExternalAlbumCatalog(expectedResults);
        var handler = new SearchExternalAlbumsHandler(catalog);
        var query = new SearchExternalAlbumsQuery(
            "  Kind of Blue  ",
            5);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken = cancellationTokenSource.Token;

        var result = await handler.HandleAsync(
            query,
            cancellationToken);

        Assert.Same(expectedResults, result);
        Assert.Equal("Kind of Blue", catalog.ReceivedQuery);
        Assert.Equal(5, catalog.ReceivedLimit);
        Assert.Equal(
            cancellationToken,
            catalog.ReceivedCancellationToken);
    }

    [Fact]
    public void Constructor_WithNullCatalog_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new SearchExternalAlbumsHandler(null!));

        Assert.Equal("catalog", exception.ParamName);
    }

    [Fact]
    public async Task HandleAsync_WithNullQuery_ShouldThrow()
    {
        var catalog = new FakeExternalAlbumCatalog([]);
        var handler = new SearchExternalAlbumsHandler(catalog);

        Task Act() =>
            handler.HandleAsync(null!, CancellationToken.None);

        var exception =
            await Assert.ThrowsAsync<ArgumentNullException>(Act);

        Assert.Equal("query", exception.ParamName);
    }

    private sealed class FakeExternalAlbumCatalog(
        IReadOnlyCollection<ExternalAlbumSearchResult> results)
        : IExternalAlbumCatalog
    {
        public string? ReceivedQuery { get; private set; }
        public int? ReceivedLimit { get; private set; }

        public CancellationToken ReceivedCancellationToken
        {
            get;
            private set;
        }

        public Task<IReadOnlyCollection<ExternalAlbumSearchResult>>
            SearchAsync(
                string query,
                int limit,
                CancellationToken cancellationToken)
        {
            ReceivedQuery = query;
            ReceivedLimit = limit;
            ReceivedCancellationToken = cancellationToken;

            return Task.FromResult(results);
        }

        public Task<ExternalAlbumSearchResult?> GetByIdAsync(
            string externalId, 
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}