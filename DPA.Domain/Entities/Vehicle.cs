namespace DPA.Domain.Entities;

public class Vehicle
{
    public Guid Id { get; private set; }
    public decimal LengthM { get; private set; }
    public string Code { get; private set; }
    public decimal WidthM { get; private set; }
    public decimal HeightM { get; private set; }
    public decimal EmptyWeightKg { get; private set; }
    public decimal FrontToRearSupportM { get; private set; }
    public decimal EmptyCgXm { get; private set; }
    public decimal FrontSupportOffsetM { get; private set; }
    public decimal MaxPayloadKg { get; private set; }
    
    
    public Vehicle(decimal lengthM, string code, decimal widthM, decimal heightM, 
        decimal emptyWeightKg, decimal frontToRearSupportM, decimal emptyCgXm, 
        decimal frontSupportOffsetM, decimal maxPayloadKg)
    {
        if (lengthM <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lengthM), "Length must be positive");
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code is required.",nameof(code));
        }

        if (widthM <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(widthM), "Width must be positive");
        }

        if (heightM <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(heightM), "Height must be positive");
        }

        if (emptyWeightKg <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(emptyWeightKg), "Empty weight kg must be positive");
        }

        if (frontToRearSupportM <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frontToRearSupportM), "FrontToRearSupportM must be positive");
        }

        if (emptyCgXm <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(emptyCgXm), "Empty cgXM must be positive");
        }

        if (frontSupportOffsetM <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frontSupportOffsetM), "FrontSupportOffsetM must be positive");
        }

        if (maxPayloadKg <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPayloadKg), "Max payload kg must be positive");
        }

        if (lengthM < frontToRearSupportM + frontSupportOffsetM)
        {
            throw new ArgumentException("Length must be at least the total of " +
                                        "FrontToRearSupportM and FrontSupportOffSetM.");
        }
        
        Id = Guid.NewGuid();
        LengthM = lengthM;
        Code = code;
        WidthM = widthM;
        HeightM = heightM;
        EmptyWeightKg = emptyWeightKg;
        FrontToRearSupportM = frontToRearSupportM;
        EmptyCgXm = emptyCgXm;
        FrontSupportOffsetM = frontSupportOffsetM;
        MaxPayloadKg = maxPayloadKg;
    }
}