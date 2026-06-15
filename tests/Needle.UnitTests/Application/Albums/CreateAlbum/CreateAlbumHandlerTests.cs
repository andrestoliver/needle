using Needle.Application.Albums;
using Needle.Application.Albums.CreateAlbum;
using Needle.Application.Albums.SearchExternalAlbums;
using Needle.Domain.Albums;

namespace Needle.UnitTests.Application.Albums.CreateAlbum;

public class CreateAlbumHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateAndPersistAlbum()
    {
        //Arrange
        var repository = new FakeAlbumRepository();
        var handler = new CreateAlbumHandler(repository);
        var command = new CreateAlbumCommand(
            "Kind of Blue",
            "Miles Davis",
            1959);
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        
        //Act
        var result = await handler.HandleAsync(
            command,
            cancellationToken);
        
        //Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(command.Title, result.Title);
        Assert.Equal(command.ArtistName, result.ArtistName);
        Assert.Equal(command.ReleaseYear, result.ReleaseYear);
        Assert.Same(result, repository.AddedAlbum);
        Assert.Equal(cancellationToken, repository.ReceivedCancellationToken);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidCommand_ShouldNotPersistAlbum()
    {
        //Arrange
        var repository = new FakeAlbumRepository();
        var handler = new CreateAlbumHandler(repository);
        var command = new CreateAlbumCommand(
            "Kind of Blue",
            null!,
            1959);
        
        // Act
        Task Act() => handler.HandleAsync(command, CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(Act);

        Assert.Equal("artistName", exception.ParamName);
        Assert.Null(repository.AddedAlbum);
    }
    
    private sealed class FakeAlbumRepository : IAlbumRepository
    {
        public Album? AddedAlbum { get; private set; }
        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task AddAsync(
            Album album,
            CancellationToken cancellationToken)
        {
            AddedAlbum = album;
            ReceivedCancellationToken = cancellationToken;
            return Task.CompletedTask;
        }
        
        public Task<Album?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<Album?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
    
    [Fact]
    public void Constructor_WithNullRepository_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new CreateAlbumHandler(null!));

        Assert.Equal("albumRepository", exception.ParamName);
    }
    
    [Fact]
    public async Task HandleAsync_WithNullCommand_ShouldThrow()
    {
        //Arrange
        var handler = new CreateAlbumHandler(new FakeAlbumRepository());

        //Act
        Task Act() => handler.HandleAsync(null!, CancellationToken.None);
        
        //Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(Act);
        Assert.Equal("command", exception.ParamName);
    }
}
