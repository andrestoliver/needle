using Needle.Application.Albums;
using Needle.Application.Albums.ImportAlbum;
using Needle.Application.Albums.SearchExternalAlbums;
using Needle.Domain.Albums;

namespace Needle.UnitTests.Application.Albums.ImportAlbum;

public sealed class ImportAlbumHandlerTests
{
    private const string ExternalId =
        "1b022e01-4da6-387b-8658-8678046e4cef";

    [Fact]
    public async Task HandleAsync_WhenAlbumAlreadyExists_ShouldReturnExistingAlbum()
    {
        var existingAlbum = Album.ImportFromMusicBrainz(
            Guid.NewGuid(),
            ExternalId,
            "Kind of Blue",
            "Miles Davis",
            1959);

        var repository = new FakeAlbumRepository(existingAlbum);
        var catalog = new FakeExternalAlbumCatalog(null);
        var handler = new ImportAlbumHandler(repository, catalog);

        var result = await handler.HandleAsync(
            new ImportAlbumCommand(ExternalId),
            CancellationToken.None);

        Assert.Equal(ImportAlbumStatus.AlreadyImported, result.Status);
        Assert.Same(existingAlbum, result.Album);
        Assert.False(catalog.WasCalled);
        Assert.Null(repository.AddedAlbum);
    }

    [Fact]
    public async Task HandleAsync_WhenExternalAlbumDoesNotExist_ShouldReturnNotFound()
    {
        var repository = new FakeAlbumRepository(null);
        var catalog = new FakeExternalAlbumCatalog(null);
        var handler = new ImportAlbumHandler(repository, catalog);

        var result = await handler.HandleAsync(
            new ImportAlbumCommand(ExternalId),
            CancellationToken.None);

        Assert.Equal(
            ImportAlbumStatus.ExternalAlbumNotFound,
            result.Status);
        Assert.Null(result.Album);
        Assert.Null(repository.AddedAlbum);
    }

    [Fact]
    public async Task HandleAsync_WhenReleaseYearIsMissing_ShouldReturnMissingReleaseYear()
    {
        var externalAlbum = new ExternalAlbumSearchResult(
            ExternalId,
            "Kind of Blue",
            "Miles Davis",
            null);

        var repository = new FakeAlbumRepository(null);
        var catalog = new FakeExternalAlbumCatalog(externalAlbum);
        var handler = new ImportAlbumHandler(repository, catalog);

        var result = await handler.HandleAsync(
            new ImportAlbumCommand(ExternalId),
            CancellationToken.None);

        Assert.Equal(
            ImportAlbumStatus.MissingReleaseYear,
            result.Status);
        Assert.Null(result.Album);
        Assert.Null(repository.AddedAlbum);
    }

    [Fact]
    public async Task HandleAsync_WithValidExternalAlbum_ShouldImportAndPersistAlbum()
    {
        var externalAlbum = new ExternalAlbumSearchResult(
            ExternalId,
            "Kind of Blue",
            "Miles Davis",
            1959);

        var repository = new FakeAlbumRepository(null);
        var catalog = new FakeExternalAlbumCatalog(externalAlbum);
        var handler = new ImportAlbumHandler(repository, catalog);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken = cancellationTokenSource.Token;

        var result = await handler.HandleAsync(
            new ImportAlbumCommand(ExternalId),
            cancellationToken);

        Assert.Equal(ImportAlbumStatus.Imported, result.Status);
        Assert.NotNull(result.Album);
        Assert.Same(result.Album, repository.AddedAlbum);
        Assert.NotEqual(Guid.Empty, result.Album.Id);
        Assert.Equal(ExternalId, result.Album.ExternalId);
        Assert.Equal(externalAlbum.Title, result.Album.Title);
        Assert.Equal(
            externalAlbum.ArtistName,
            result.Album.ArtistName);
        Assert.Equal(
            externalAlbum.FirstReleaseYear,
            result.Album.ReleaseYear);

        Assert.Equal(
            cancellationToken,
            repository.ReceivedCancellationToken);
        Assert.Equal(
            cancellationToken,
            catalog.ReceivedCancellationToken);
    }

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new ImportAlbumHandler(
                null!,
                new FakeExternalAlbumCatalog(null)));

        Assert.Equal("albumRepository", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullCatalog_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new ImportAlbumHandler(
                new FakeAlbumRepository(null),
                null!));

        Assert.Equal("catalog", exception.ParamName);
    }

    [Fact]
    public async Task HandleAsync_WithNullCommand_ShouldThrow()
    {
        var handler = new ImportAlbumHandler(
            new FakeAlbumRepository(null),
            new FakeExternalAlbumCatalog(null));

        Task Act() =>
            handler.HandleAsync(null!, CancellationToken.None);

        var exception =
            await Assert.ThrowsAsync<ArgumentNullException>(Act);

        Assert.Equal("command", exception.ParamName);
    }

    private sealed class FakeAlbumRepository(
        Album? existingAlbum) : IAlbumRepository
    {
        public Album? AddedAlbum { get; private set; }

        public CancellationToken ReceivedCancellationToken
        {
            get;
            private set;
        }

        public Task AddAsync(
            Album album,
            CancellationToken cancellationToken)
        {
            AddedAlbum = album;
            ReceivedCancellationToken = cancellationToken;

            return Task.CompletedTask;
        }

        public Task<Album?> GetByExternalIdAsync(
            string externalId,
            CancellationToken cancellationToken)
        {
            ReceivedCancellationToken = cancellationToken;
            return Task.FromResult(existingAlbum);
        }

        public Task<Album?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeExternalAlbumCatalog(
        ExternalAlbumSearchResult? externalAlbum)
        : IExternalAlbumCatalog
    {
        public bool WasCalled { get; private set; }

        public CancellationToken ReceivedCancellationToken
        {
            get;
            private set;
        }

        public Task<ExternalAlbumSearchResult?> GetByIdAsync(
            string externalId,
            CancellationToken cancellationToken)
        {
            WasCalled = true;
            ReceivedCancellationToken = cancellationToken;

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
}