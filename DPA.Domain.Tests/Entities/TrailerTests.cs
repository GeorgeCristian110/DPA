using DPA.Domain.Entities;

namespace DPA.Domain.Tests.Entities;

public class TrailerTests
{
    [Fact]
    public void Constructor_ValidValues_CreatesTrailer()
    {
        //Arrange
        var code = "MM-09";
        
        //Act
        var vehicle = new Vehicle(25.5m, code, 3.30m, 2.5m, 7000m,
            7.6m, 6.2m, 1m, 24500m);
        
        //Assert
        Assert.Equal("MM-09", vehicle.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_InvalidCode_ThrowsException(string? code)
    {
       Assert.Throws<ArgumentException>(() =>
            new Vehicle(25.5m, code!, 3.30m, 2.5m, 7000m,
            7.6m, 6.2m, 1m, 24500m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidLength_ThrowsException(decimal length)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Vehicle(length, "MM-01", 3.30m, 2.5m, 7000m,
                7.6m, 6.2m, 1m, 24500m));
    }
}