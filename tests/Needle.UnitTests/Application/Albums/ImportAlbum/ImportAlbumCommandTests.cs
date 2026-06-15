using Needle.Application.Albums.ImportAlbum;

namespace Needle.UnitTests.Application.Albums.ImportAlbum;

public class ImportAlbumCommandTests
{
    [Fact]
    public void Command_WithValidData_ShouldAccept()
    {
        //Arrange
        const string externalId = "1b022e01-4da6-387b-8658-8678046e4cef";
        
        //Act
        var command = new ImportAlbumCommand(externalId);
        
        //Assert
        Assert.Equal(externalId, command.ExternalId);
    }

    [Fact]
    public void Command_ExternalId_ShouldBeTrimmed()
    {
        //Arrange
        const string externalId = "  1b022e01-4da6-387b-8658-8678046e4cef  ";
        
        //Act
        var command = new ImportAlbumCommand(externalId);
        
        //Assert
        Assert.Equal("1b022e01-4da6-387b-8658-8678046e4cef", command.ExternalId);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Command_ExternalId_ShouldRejectEmptyOrWhiteSpace(string externalId)
    {
        //Act
        Action act = () => new ImportAlbumCommand(externalId);
        
        //Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("externalId", exception.ParamName);
    }

    [Fact]
    public void Command_WithNullExternalId_ShouldThrow()
    {
        //Arrange
        string externalId = null!;
        
        //Act
        Action act = () => new ImportAlbumCommand(externalId);
        
        //Assert
        var exception = Assert.Throws<ArgumentNullException>(act);
        Assert.Equal("externalId", exception.ParamName);
    }

    [Fact]
    public void Command_WithGuidWithoutHyphens_ShouldThrow()
    {
        //Arrange
        const string externalId = "1b022e014da6387b86588678046e4cef";
        
        //Act
        Action act = () => new ImportAlbumCommand(externalId);
        
        //Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("externalId", exception.ParamName);
    }
    
    [Fact]
    public void Command_WithInvalidExternalId_ShouldThrow()
    {
        //Act
        Action act = () => new ImportAlbumCommand("invalid-id");

        //Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("externalId", exception.ParamName);
    }
}