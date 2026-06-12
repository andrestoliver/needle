using Needle.Domain.Albums;

namespace Needle.UnitTests.Domain.Albums;

public class AlbumTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateAlbum()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var album = Album.CreateManual(
            id,
            "Kind of Blue",
            "Miles Davis",
            1959);

        // Assert
        Assert.Equal(id, album.Id);
        Assert.Equal("Kind of Blue", album.Title);
        Assert.Equal("Miles Davis", album.ArtistName);
        Assert.Equal(1959, album.ReleaseYear);
    }
    
    [Fact]
    public void Id_ShouldNotBeEmpty()
    {
        //Arrange
        var id = Guid.Empty;

        //Act
        Action act = () => Album.CreateManual(id, "Kind of Blue", "Miles Davis", 1959);

        //Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Title_ShouldBeTrimmed()
    {
        //Arrange
        var title = "  Kind of Blue  ";

        //Act
        var album = Album.CreateManual(Guid.NewGuid(), title, "Miles Davis", 1959);

        //Assert
        Assert.Equal("Kind of Blue", album.Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Title_ShouldRejectEmptyOrWhiteSpace(string title)
    {
        //Act
        Action act = () => Album.CreateManual(Guid.NewGuid(), title, "Miles Davis", 1959);

        //Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("title", exception.ParamName);
    }

    [Fact]
    public void Title_ShouldRejectNull()
    {
        //Arrange
        string title = null!;

        //Act
        Action act = () => Album.CreateManual(Guid.NewGuid(), title, "Miles Davis", 1959);

        //Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("title", exception.ParamName);
    }

    [Fact]
    public void ArtistName_ShouldBeTrimmed()
    {
        //Arrange
        var artistName = "  Miles Davis  ";

        //Act
        var album = Album.CreateManual(Guid.NewGuid(), "Kind of Blue", artistName, 1959);

        //Assert
        Assert.Equal("Miles Davis", album.ArtistName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ArtistName_ShouldRejectEmptyOrWhiteSpace(string artistName)
    {
        //Act
        Action act = () => Album.CreateManual(Guid.NewGuid(), "Kind of Blue", artistName, 1959);

        //Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("artistName", exception.ParamName);
    }

    [Fact]
    public void ArtistName_ShouldRejectNull()
    {
        //Arrange
        string artistName = null!;

        //Act
        Action act = () => Album.CreateManual(Guid.NewGuid(), "Kind of Blue", artistName, 1959);

        //Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("artistName", exception.ParamName);
    }
    
    [Fact]
    public void ReleaseYear_When1876_ShouldBeRejected()
    {
        //Arrange
        var releaseYear = 1876;

        //Act
        Action act = () => Album.CreateManual(Guid.NewGuid(), "Kind of Blue", "Miles Davis", releaseYear);

        //Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(act);
        Assert.Equal("releaseYear", exception.ParamName);
    }
    
    [Fact]
    public void ReleaseYear_When1877_ShouldBeAccepted()
    {
        // Arrange
        const int releaseYear = 1877;

        // Act
        var album = Album.CreateManual(
            Guid.NewGuid(),
            "Kind of Blue",
            "Miles Davis",
            releaseYear);

        // Assert
        Assert.Equal(releaseYear, album.ReleaseYear);
    }

    [Fact]
    public void CreateManual_ShouldCreateAlbumWithoutExternalId()
    {
        //Act 
        var album = Album.CreateManual(Guid.NewGuid(), "Kind of Blue", "Miles Davis", 1959);
        
        //Assert
        Assert.Null(album.ExternalId);
    }

    [Fact]
    public void ImportFromMusicBrainz_ShouldCreateAlbumWithTrimmedExternalId()
    {
        //Arrange
        const string externalId =
            "  1b022e01-4da6-387b-8658-8678046e4cef  ";
        
        //Act
        var album = Album.ImportFromMusicBrainz(
            Guid.NewGuid(), 
            externalId, 
            "Kind of Blue", 
            "Miles Davis", 
            1959);
        
        //Assert
        Assert.Equal("1b022e01-4da6-387b-8658-8678046e4cef", album.ExternalId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ImportFromMusicBrainz_WithInvalidExternalId_ShouldThrow(
        string externalId)
    {
        //Act
        void Act() => Album.ImportFromMusicBrainz(
            Guid.NewGuid(), 
            externalId, 
            "Kind of Blue", 
            "Miles Davis", 
            1959);

        //Assert
        var exception = Assert.Throws<ArgumentException>(Act);
        Assert.Equal("externalId", exception.ParamName);
    }
    
    [Fact]
    public void ImportFromMusicBrainz_WithNullExternalId_ShouldThrow()
    {
        //Arrange
        string? externalId = null!;
        
        //Act
        void Act() => Album.ImportFromMusicBrainz(
            Guid.NewGuid(), 
            externalId, 
            "Kind of Blue", 
            "Miles Davis", 
            1959);

        //Assert
        var exception = Assert.Throws<ArgumentNullException>(Act);
        Assert.Equal("externalId", exception.ParamName);
    }
    
    [Fact]
    public void ImportFromMusicBrainz_WithInvalidExternalIdFormat_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => Album.ImportFromMusicBrainz(
                Guid.NewGuid(),
                "invalid-id",
                "Kind of Blue",
                "Miles Davis",
                1959));

        Assert.Equal("externalId", exception.ParamName);
    }
}