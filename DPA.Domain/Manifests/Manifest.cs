using DPA.Domain.Cargo;
using DPA.Domain.Shared;
using DPA.Domain.Vehicles;

namespace DPA.Domain.Manifests;

public class Manifest
{
    //Manifest id and foreign keys Id's
    public Guid Id { get; private set; }
    public Guid DestinationId { get; private set; }
    public Guid? VehicleId { get; private set; }
    
    //The status for the manifest
    public ManifestStatus Status { get; private set; }
    
    //Time measurements for delivery and statuses
    public DateTimeOffset RequiredArrivalAt { get; private set; }
    public DateTimeOffset? PlanDueAt { get; private set; }
    public DateTimeOffset? TruckArrivedAt { get; private set; }
    public DateTimeOffset? DispatchedAt { get; private set; }
    
    //The id and name by the planner who confirms the manifest as done
    public Guid? ConfirmedByUserId {get; private set;}
    public string? ConfirmedByName {get; private set;}
    
    //The id and name of the team lead who approves the plan made
    public Guid? ApprovedByUserId {get; private set;}
    public string? ApprovedByName {get; private set;}

    private readonly List<Placement> _placements = new();
    public IReadOnlyCollection<Placement> Placements => _placements;

    public Manifest(Guid destinationId, DateTimeOffset requiredArrivalAt)
    {
        if (destinationId == Guid.Empty)
        {
            throw new ArgumentException("Destination ID cannot be empty", nameof(destinationId));
        }

        if (requiredArrivalAt == default)
        {
            throw new ArgumentException("Required arrival time must be set", nameof(requiredArrivalAt));
        }
        
        Id = Guid.NewGuid();
        DestinationId = destinationId;
        RequiredArrivalAt = requiredArrivalAt;
        Status = ManifestStatus.AwaitingPallets;
    }
    
    /// <summary>
    /// Places a pallet on the manifest's vehicle at the given position.
    /// Fails with a Result when a placement rule is broken: wrong status, pallet from another
    /// manifest, already placed, too tall, too heavy, outside the vehicle or overlapping another pallet.
    /// </summary>
    /// <param name="vehicle">The vehicle assigned to this manifest.</param>
    /// <param name="cargoItem">The pallet to place.</param>
    /// <param name="xm">Center of the pallet along the vehicle,
    /// in metres from the front support.</param>
    /// <param name="ym">Center of the pallet across the vehicle,
    /// in metres from the center line (left is negative).</param>
    /// <param name="isRotated">True if the pallet is turned 90°,
    /// swapping its length and width.</param>
    /// <returns>Success, or a Conflict or RuleViolation failure explaining
    /// which check failed.</returns>
    /// <exception cref="ArgumentException">When the vehicle is not the one assigned
    /// to this manifest.</exception>
    public Result PlaceCargo(Vehicle vehicle, CargoItem cargoItem, decimal xm, decimal ym,
        bool isRotated)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        ArgumentNullException.ThrowIfNull(cargoItem);
       
        if (vehicle.Id != VehicleId)
        {
            throw new ArgumentException(
                "The vehicle does not match the manifest's assigned vehicle", nameof(vehicle));
        }
        
        if (Status != ManifestStatus.UnderPlanning)
        {
            return Result.Failure
                (ErrorType.Conflict, 
                    $"Cannot place pallets, the manifest is {Status}.");
        }

        if (cargoItem.ManifestId != Id)
        {
            return Result.Failure
                (ErrorType.RuleViolation,
                    $"Pallet {cargoItem.PalletCode} does not belong to this manifest");
        }

        if (_placements.Any(p => p.CargoItemId == cargoItem.Id))
        {
            return Result.Failure(ErrorType.Conflict,
                $"Pallet {cargoItem.PalletCode} already has a placement");
        }

        if (cargoItem.HeightM > vehicle.HeightM)
        {
            return Result.Failure(ErrorType.RuleViolation,
                $"Pallet {cargoItem.PalletCode} is too tall for this vehicle");
        }

        var placedWeight = _placements.Sum(p => p.WeightKg);

        if (placedWeight + cargoItem.WeightKg > vehicle.MaxPayloadKg)
        {
            return Result.Failure(ErrorType.RuleViolation,
            $"Pallet {cargoItem.PalletCode} is too heavy for this vehicle ");
        }

        var alongLength = isRotated ? cargoItem.WidthM : cargoItem.LengthM;
        var acrossWidth = isRotated ? cargoItem.LengthM : cargoItem.WidthM;
      
        var noseWall = -vehicle.FrontSupportOffsetM;
        var rearDoors = vehicle.LengthM - vehicle.FrontSupportOffsetM;
       
        var frontEdge = xm - alongLength / 2;
        var rearEdge = xm + alongLength / 2;

        var leftWall = -vehicle.WidthM / 2;
        var rightWall = vehicle.WidthM / 2;
        
        var leftEdge = ym - acrossWidth / 2;
        var rightEdge = ym + acrossWidth / 2;

        if (frontEdge < noseWall || rearEdge > rearDoors)
        {
            return Result.Failure(ErrorType.RuleViolation,
                $"Pallet {cargoItem.PalletCode} it's out of boundaries at this position." +
                $" Please try again on a new position. ");
        }

        if (leftEdge < leftWall || rightEdge > rightWall)
        {
            return Result.Failure(ErrorType.RuleViolation,
                $"Pallet {cargoItem.PalletCode} stick out the past the sides " +
                $" of the vehicle at this position." +
                $" Please try again on a new position. ");
        }

        var overlaps = _placements.Any(p =>
        {
            var pAlong = p.IsRotated ? p.WidthM : p.LengthM;
            var pAcross = p.IsRotated ? p.LengthM : p.WidthM;

            var pFront = p.Xm - pAlong / 2;
            var pRear = p.Xm + pAlong / 2;
            
            var pLeft = p.Ym - pAcross / 2;
            var pRight = p.Ym + pAcross / 2;

            var overlapX = frontEdge < pRear && pFront < rearEdge;
            var overlapY = leftEdge < pRight && pLeft < rightEdge;

            return overlapX && overlapY;
        });

        if (overlaps)
        {
            return Result.Failure(ErrorType.Conflict,
                $"Pallet  {cargoItem.PalletCode} is overlapping with " +
                $" another pallet at this position.");
        }
        
        _placements.Add(new Placement(cargoItem, xm, ym, isRotated));
        return Result.Success();
    }
}