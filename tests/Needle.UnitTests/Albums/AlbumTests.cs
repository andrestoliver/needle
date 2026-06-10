using Needle.Domain.Albums;

namespace Needle.UnitTests.Albums;

public class AlbumTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateAlbum()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var album = new Album(
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
        Action act = () => new Album(id, "Kind of Blue", "Miles Davis", 1959);

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
        var album = new Album(Guid.NewGuid(), title, "Miles Davis", 1959);

        //Assert
        Assert.Equal("Kind of Blue", album.Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Title_ShouldRejectEmptyOrWhiteSpace(string title)
    {
        //Act
        Action act = () => new Album(Guid.NewGuid(), title, "Miles Davis", 1959);

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
        Action act = () => new Album(Guid.NewGuid(), title, "Miles Davis", 1959);

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
        var album = new Album(Guid.NewGuid(), "Kind of Blue", artistName, 1959);

        //Assert
        Assert.Equal("Miles Davis", album.ArtistName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ArtistName_ShouldRejectEmptyOrWhiteSpace(string artistName)
    {
        //Act
        Action act = () => new Album(Guid.NewGuid(), "Kind of Blue", artistName, 1959);

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
        Action act = () => new Album(Guid.NewGuid(), "Kind of Blue", artistName, 1959);

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
        Action act = () => new Album(Guid.NewGuid(), "Kind of Blue", "Miles Davis", releaseYear);

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
        var album = new Album(
            Guid.NewGuid(),
            "Kind of Blue",
            "Miles Davis",
            releaseYear);

        // Assert
        Assert.Equal(releaseYear, album.ReleaseYear);
    }
}