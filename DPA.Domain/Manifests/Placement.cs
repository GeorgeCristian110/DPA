namespace DPA.Domain.Manifests;

 public class Placement
{
    public Guid Id { get; private set; }
    public Guid CargoItemId { get; private set; }
    public decimal Xm { get; private set; }
    public decimal Ym { get; private set; }
    public bool IsRotated { get; private set; }


    internal Placement(Guid cargoItemId, decimal xM, decimal yM, bool isRotated)
    {
        if (cargoItemId == Guid.Empty)
        {
            throw new ArgumentException("Cargo item id can not be empty",  nameof(cargoItemId));
        }
        
        
        
        Id = Guid.NewGuid();
        CargoItemId = cargoItemId;
        Xm = xM;
        Ym = yM;
        IsRotated = isRotated;
    }
}