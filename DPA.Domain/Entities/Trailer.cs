namespace DPA.Domain.Entities;

public class Trailer
{
    public Guid Id { get; private set; }
    public decimal LengthM { get; private set; }
    public string Code { get; private set; }
    public decimal WidthM { get; private set; }
    public decimal HeightM { get; private set; }
    public decimal EmptyWeightKg { get; private set; }
    public decimal KingpinToTandemM { get; private set; }
    public decimal EmptyCgXm { get; private set; }
    public decimal KingpinOffsetM { get; private set; }
    public decimal MaxPayloadKg { get; private set; }
    
    
    public Trailer(decimal lengthM, string code, decimal widthM, decimal heightM, 
        decimal emptyWeightKg, decimal kingpinToTandemM, decimal emptyCgXm, 
        decimal kingpinOffsetM, decimal maxPayloadKg)
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

        if (kingpinToTandemM <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(kingpinToTandemM), "KingpinToTandem must be positive");
        }

        if (emptyCgXm <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(emptyCgXm), "Empty cgXM must be positive");
        }

        if (kingpinOffsetM <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(kingpinOffsetM), "KingpinOffsetM must be positive");
        }

        if (maxPayloadKg <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPayloadKg), "Max payload kg must be positive");
        }

        if (lengthM < kingpinToTandemM + kingpinOffsetM)
        {
            throw new ArgumentException("Length must be at least the total of " +
                                        "kingpinToTandemM and kingpinOffsetM.");
        }
        
        Id = Guid.NewGuid();
        LengthM = lengthM;
        Code = code;
        WidthM = widthM;
        HeightM = heightM;
        EmptyWeightKg = emptyWeightKg;
        KingpinToTandemM = kingpinToTandemM;
        EmptyCgXm = emptyCgXm;
        KingpinOffsetM = kingpinOffsetM;
        MaxPayloadKg = maxPayloadKg;
    }
}