using System.Globalization;
using Needle.Domain.Reviews;

namespace Needle.UnitTests.Domain.Reviews;

public class RatingTests
{
    [Theory]
    [InlineData("0.5")]
    [InlineData("1.0")]
    [InlineData("2.5")]
    [InlineData("5.0")]
    public void Constructor_WithValidValue_ShouldCreateRating(string valueText)
    {
        var value = decimal.Parse(
            valueText,
            CultureInfo.InvariantCulture);

        var rating = new Rating(value);

        Assert.Equal(value, rating.Value);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("5.5")]
    public void Constructor_WithValueOutsideRange_ShouldThrow(string valueText)
    {
        var value = decimal.Parse(
            valueText,
            CultureInfo.InvariantCulture);

        void Act() => new Rating(value);

        Assert.Throws<ArgumentOutOfRangeException>(Act);
    }

    [Theory]
    [InlineData("1.2")]
    [InlineData("4.7")]
    public void Constructor_WithInvalidIncrement_ShouldThrow(string valueText)
    {
        var value = decimal.Parse(
            valueText,
            CultureInfo.InvariantCulture);

        void Act() => new Rating(value);

        Assert.Throws<ArgumentException>(Act);
    }
    
    [Fact]
    public void Rating_WithTheSameValue_ShouldBeEqual()
    {
        var first = new Rating(1);
        var second = new Rating(1);
     
        Assert.Equal(first, second);
    }
}