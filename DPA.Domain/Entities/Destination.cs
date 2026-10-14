namespace DPA.Domain.Entities;

public class Destination
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Address { get; private set; }
    public string ZipCode { get; private set; }
    public string City { get; private set; }
    public TimeSpan DrivingTime { get; private set; }

    public Destination(string name, string address,
        string zipCode, string city, TimeSpan drivingTime)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("The destination name can not be null or empty",nameof(name));
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            throw new ArgumentException("The destination address can not be null or empty",nameof(address));
        }

        if (string.IsNullOrWhiteSpace(zipCode))
        {
            throw new ArgumentException("The destination zip code can not be null or empty",nameof(zipCode));
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            throw new ArgumentException("The destination city can not be null or empty",nameof(city));
        }
        

        Id = Guid.NewGuid();
        Name = name;
        Address = address;
        ZipCode = zipCode;
        City = city;
        UpdateDrivingTime(drivingTime);
     
    }

    public void UpdateDrivingTime(TimeSpan drivingTime)
    {
        if (drivingTime <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(drivingTime),"DrivingTime must be greater than zero");
        }
        
        DrivingTime = drivingTime;
    }
}