namespace DPA.Domain.Cargo;

public class CargoItem
{
    public Guid Id { get; private set; }
    public string PalletCode { get; private set; }
    public decimal LengthM { get; private set; }
    public decimal WidthM { get; private set; }
    public decimal HeightM { get; private set; }
    public decimal WeightKg { get; private set; }
    
    //Enums and foreign key relationships.
    public Stability Stability { get; private set; }
    public CargoStatus Status { get; private set; }
    public Guid? ManifestId { get; private set; }
    
    //Date and time according to utc to remove daylight changes.
    public DateTimeOffset? ArrivedOnDockAt { get; private set; }

    public CargoItem(string palletCode, decimal lengthM, decimal widthM, decimal heightM, 
        decimal weightKg, Stability stability)
    {

        if (string.IsNullOrWhiteSpace(palletCode))
        {
            throw new ArgumentException("palletCode cannot be null or empty", nameof(palletCode));
        }

        if (lengthM <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lengthM), "Length m must be greater than zero");
        }

        if (widthM <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(widthM), "Width m must be greater than zero");
        }

        if (heightM <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(heightM), "Height m must be greater than zero");
        }

        if (weightKg <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weightKg), "Weight kg must be greater than zero");
        }

        if (!Enum.IsDefined(stability))
        {
            throw new ArgumentOutOfRangeException(nameof(stability), "Stability must be a valid value." +
                                                                     " Valid values are Stable or Unstable.");
        }
        
        Id = Guid.NewGuid();
        PalletCode = palletCode;
        LengthM = lengthM;
        WidthM = widthM;
        WeightKg = weightKg;
        Stability = stability;
        HeightM = heightM;
        Status = CargoStatus.InStorage;
    }
}