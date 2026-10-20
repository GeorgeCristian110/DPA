using DPA.Domain.Vehicles;

namespace DPA.Domain.Tests.Vehicles;

public class VehicleTests
{
    private readonly VehicleProfile _semiProfile = new("Semi-trailer",
        0.5m, 2.0m);
    
    [Fact]
    public void Constructor_ValidValues_CreatesTrailer()
    {
        //Arrange
        var code = "MM-09";
        
        //Act
        var vehicle = new Vehicle(_semiProfile, 13.6m, code, 3.30m, 2.5m, 7000m,
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
            new Vehicle(_semiProfile, 2.2m, code!, 3.30m, 2.5m, 7000m,
            7.6m, 6.2m, 1m, 24500m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidLength_ThrowsException(decimal length)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Vehicle(_semiProfile, length , "MM-01",3.30m, 2.5m, 7000m,
                7.6m, 6.2m, 1m, 24500m));
    }
}