using DPA.Domain.Cargo;

namespace DPA.Domain.Manifests;

 public class Placement
{
    public Guid Id { get; private set; }
    public Guid CargoItemId { get; private set; }
    public decimal Xm { get; private set; }
    public decimal Ym { get; private set; }
    public bool IsRotated { get; private set; }
    
    public decimal WeightKg { get; private set; }
    public decimal LengthM { get; private set; }
    public decimal WidthM { get; private set; }
    public decimal HeightM { get; private set; }

    #pragma warning disable CS8618
    private Placement() { }   // for EF Core
    #pragma warning restore CS8618

    internal Placement(CargoItem cargoItem, decimal xM, decimal yM, bool isRotated)
    {
       ArgumentNullException.ThrowIfNull(cargoItem);
       
        Id = Guid.NewGuid();
        CargoItemId = cargoItem.Id;
        Xm = xM;
        Ym = yM;
        WeightKg = cargoItem.WeightKg;
        LengthM = cargoItem.LengthM;
        WidthM = cargoItem.WidthM;
        HeightM = cargoItem.HeightM;
        IsRotated = isRotated;
    }
}