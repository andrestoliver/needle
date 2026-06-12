using Needle.Application.Albums;
using Needle.Application.Albums.GetAlbumById;
using Needle.Domain.Albums;

namespace Needle.UnitTests.Application.Albums.GetAlbumById;

public class GetAlbumByIdHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidId_ReturnsAlbum()
    {
        //Arrange
        var album = Album.CreateManual(
            Guid.NewGuid(),
            "Kind of Blue",
            "Miles Davis",
            1959);
        var repository = new FakeAlbumRepository(album);
        var handler =  new GetAlbumByIdHandler(repository);
        var query = new GetAlbumByIdQuery(album.Id);
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        
        //Act
        var result = await handler.HandleAsync(
            query, 
            cancellationToken);
        
        //Assert
        Assert.NotNull(result);
        Assert.Same(album, result);
        Assert.Equal(album.Id, repository.ReceivedId);
        Assert.Equal(cancellationToken, repository.ReceivedCancellationToken);
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumDoesNotExist_ReturnsNull()
    {
        // Arrange
        var repository = new FakeAlbumRepository(null);
        var handler = new GetAlbumByIdHandler(repository);
        var query = new GetAlbumByIdQuery(Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
        Assert.Equal(query.Id, repository.ReceivedId);
    }

    [Fact]
    public async Task HandleAsync_WithNullQuery_ThrowsArgumentNullException()
    {
        // Arrange
        var handler = new GetAlbumByIdHandler(new FakeAlbumRepository(null));

        // Act
        Task Act() => handler.HandleAsync(null!, CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(Act);
        Assert.Equal("query", exception.ParamName);
    }

    [Fact]
    public void QueryConstructor_WithEmptyId_ThrowsArgumentException()
    {
        // Act
        var exception = Assert.Throws<ArgumentException>(
            () => new GetAlbumByIdQuery(Guid.Empty));

        // Assert
        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void HandlerConstructor_WithNullRepository_ThrowsArgumentNullException()
    {
        // Act
        var exception = Assert.Throws<ArgumentNullException>(
            () => new GetAlbumByIdHandler(null!));

        // Assert
        Assert.Equal("albumRepository", exception.ParamName);
    }
    
    private sealed class FakeAlbumRepository : IAlbumRepository
    {
        private readonly Album? _album;

        public FakeAlbumRepository(Album? album)
        {
            _album = album;
        }

        public Guid? ReceivedId { get; private set; }
        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<Album?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            ReceivedId = id;
            ReceivedCancellationToken = cancellationToken;

            return Task.FromResult(_album);
        }

        public Task AddAsync(
            Album album,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
