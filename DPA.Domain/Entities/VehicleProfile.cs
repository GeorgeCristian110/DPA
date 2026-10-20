namespace DPA.Domain.Entities;

public class VehicleProfile
{
    public Guid Id { get; private set; }
    public string Name {get; private set;}
    public decimal MinFrontOffsetM  { get; private set; }
    public decimal MaxFrontOffsetM { get; private set; }

    public VehicleProfile(string name, decimal minFrontOffsetM, decimal maxFrontOffsetM)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.",nameof(name));
        }

        if (minFrontOffsetM > maxFrontOffsetM)
        {
            throw new ArgumentException("Min front offset must be less than or equal " +
                                        "than max front offset." , nameof(minFrontOffsetM) );
        }
        
        
        Id = Guid.NewGuid();
        Name = name;
        MinFrontOffsetM = minFrontOffsetM;
        MaxFrontOffsetM = maxFrontOffsetM;
    }
}