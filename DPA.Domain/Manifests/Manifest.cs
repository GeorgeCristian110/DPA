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

    private readonly List<Rejection> _rejections = new();
    public IReadOnlyCollection<Rejection> Rejections => _rejections;

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
                $"Pallet {cargoItem.PalletCode} is out of boundaries at this position." +
                $" Please try again on a new position. ");
        }

        if (leftEdge < leftWall || rightEdge > rightWall)
        {
            return Result.Failure(ErrorType.RuleViolation,
                $"Pallet {cargoItem.PalletCode} sticks out past the sides" +
                $" of the vehicle at this position." +
                $" Please try again on a new position.");
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
                $"Pallet {cargoItem.PalletCode} is overlapping with " +
                $" another pallet at this position.");
        }
        
        _placements.Add(new Placement(cargoItem, xm, ym, isRotated));
        return Result.Success();
    }

    /// <summary>
    /// Marks that all pallets are on the dock and the manifest is ready for a planner.
    /// Fails with a Result when the manifest is not awaiting pallets, has no pallets,
    /// or when some pallets are not on the dock yet.
    /// </summary>
    /// <param name="pallets">All pallets that belong to this manifest, loaded by the caller.</param>
    /// <returns>Success, or a Conflict or RuleViolation failure explaining
    /// which check failed.</returns>
    /// <exception cref="ArgumentException">When a pallet belongs to another manifest.</exception>
    public Result MarkReadyToPlan(IReadOnlyList<CargoItem> pallets)
    {
        ArgumentNullException.ThrowIfNull(pallets);

        if (pallets.Any(p => p.ManifestId != Id))
        {
            throw new ArgumentException
                ("The list contains a pallet from another manifest", nameof(pallets));
        }

        if (Status != ManifestStatus.AwaitingPallets)
        {
            return Result.Failure(ErrorType.Conflict,
                $"Cannot prepare for planning, the manifest is {Status} ");
        }

        if (pallets.Count == 0)
        {
            return Result.Failure(ErrorType.RuleViolation,
                "Cannot prepare for planning, the manifest has no pallets.");
        }

        var notOnDock = pallets.Count(p => p.Status != CargoStatus.OnDock);

        if (notOnDock > 0)
        {
            return Result.Failure(ErrorType.RuleViolation,
                $"The number of pallets not on dock yet is {notOnDock}." +
                $" Please try again later");
        }

        Status = ManifestStatus.AwaitingPlanning;
        return Result.Success();
    }

    /// <summary>
    /// Starts planning the manifest and assigns the vehicle it will be loaded on.
    /// Fails with a Conflict if the manifest is not awaiting planning.
    /// </summary>
    /// <param name="vehicle">The vehicle the planner chose for this manifest.</param>
    /// <returns>Success, or a Conflict failure with the manifest's current status.</returns>
    public Result StartPlanning(Vehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);

        if (Status != ManifestStatus.AwaitingPlanning)
        {
            return Result.Failure(ErrorType.Conflict,
                $"Cannot start planning the manifest, it is {Status}");
        }
        
        VehicleId = vehicle.Id;

        Status = ManifestStatus.UnderPlanning;
        return Result.Success();
    }
    
    
    /// <summary>
    /// Confirms the finished plan and sends the manifest to loading.
    /// Fails with a Result when the manifest is not under planning, has no pallets,
    /// or when some pallets have no placement yet.
    /// </summary>
    /// <param name="pallets">All pallets that belong to this manifest, loaded by the caller.</param>
    /// <param name="plannerId">The id of the planner who confirms the plan.</param>
    /// <param name="plannerName">The name of the planner who confirms the plan.</param>
    /// <returns>Success, or a Conflict or RuleViolation failure explaining
    /// which check failed.</returns>
    /// <exception cref="ArgumentException">When the planner id or name is empty,
    /// or a pallet belongs to another manifest.</exception>
    public Result Confirm(IReadOnlyCollection<CargoItem> pallets, Guid plannerId, string plannerName)
    {
        ArgumentNullException.ThrowIfNull(pallets);


        if (plannerId == Guid.Empty)
        {
            throw new ArgumentException("The planner id cannot be empty", 
                nameof(plannerId));
        }

        if (string.IsNullOrWhiteSpace(plannerName))
        {
            throw new ArgumentException("The planner name can not be null or white space",
                nameof(plannerName));
        }

        if (pallets.Any(p => p.ManifestId != Id))
        {
            throw new ArgumentException
                ("The list contains a pallet from another manifest", nameof(pallets));
        }
        
        if (Status != ManifestStatus.UnderPlanning)
        {
            return Result.Failure(ErrorType.Conflict,
                $"Cannot confirm the manifest, it is {Status}");
        }
        
        if (pallets.Count == 0)
        {
            return Result.Failure(ErrorType.RuleViolation,
                "Cannot confirm the manifest, it has no pallets.");
        }
        
        var unplaced = pallets
            .Count(p => !_placements.Any(pl => pl.CargoItemId == p.Id));

        if (unplaced > 0)
        {
            return Result.Failure(ErrorType.RuleViolation,
                $"Cannot confirm the manifest, there are" +
                $" {unplaced} pallets that are not placed");
        }
        
        ConfirmedByUserId = plannerId;
        ConfirmedByName = plannerName;
        Status = ManifestStatus.SentToLoading;
        return Result.Success();
    }
    
    /// <summary>
    /// Marks that all pallets have arrived at the loading area and the plan
    /// is waiting for the team lead's approval.
    /// Fails with a Conflict if the manifest is not sent to loading.
    /// </summary>
    /// <returns>Success, or a Conflict failure with the manifest's current status.</returns>
    public Result MarkAtLoadingArea()
    {
        if (Status != ManifestStatus.SentToLoading)
        {
            return Result.Failure(ErrorType.Conflict,
                $"Cannot mark the manifest as at the loading area, it is {Status}");
        }

        Status = ManifestStatus.AwaitingConfirmation;
        return Result.Success();
    }
    
    /// <summary>
    /// Records when the manifest's truck arrived at the dock.
    /// Fails if no vehicle is assigned yet, or if the truck has already arrived.
    /// </summary>
    /// <param name="now">The current time in UTC, passed in by the caller so tests can choose it.</param>
    /// <returns>Success, or a RuleViolation or Conflict failure explaining which check failed.</returns>
    public Result MarkTruckArrived(DateTimeOffset now)
    {
        if (VehicleId is null)
        {
            return Result.Failure(ErrorType.RuleViolation,
                "Cannot mark the truck as arrived," +
                " no vehicle is assigned to this manifest");
        }

        if (TruckArrivedAt is not null)
        {
            return Result.Failure(ErrorType.Conflict,
                $"The truck already arrived at {TruckArrivedAt}");
        }

        TruckArrivedAt = now;
        return Result.Success();
    }

    
    /// <summary>
    /// Approves the plan and starts loading.
    /// Fails with a Result when the manifest is not awaiting confirmation, when the team lead
    /// is the same person who confirmed the plan, or when the truck has not arrived yet.
    /// </summary>
    /// <param name="teamLeadId">The id of the team lead who approves the plan.</param>
    /// <param name="teamLeadName">The name of the team lead who approves the plan.</param>
    /// <returns>Success, or a Conflict or RuleViolation failure explaining
    /// which check failed.</returns>
    /// <exception cref="ArgumentException">When the team lead id is empty
    /// or the name is blank.</exception>
    public Result Approve(Guid teamLeadId, string teamLeadName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(teamLeadName);

        if (teamLeadId == Guid.Empty)
        {
            throw new ArgumentException("The team leader id cannot be empty", 
                nameof(teamLeadId));
        }

        if (Status != ManifestStatus.AwaitingConfirmation)
        {
            return Result.Failure(ErrorType.Conflict,
            $"Cannot approve the plan, it is {Status}");
        }

        if (ConfirmedByUserId == teamLeadId)
        {
            return Result.Failure(ErrorType.RuleViolation,
                "Cannot approve the plan, you confirmed it yourself." +
                " Another team lead must approve it.");
        }

        if (TruckArrivedAt is null)
        {
            return Result.Failure(ErrorType.RuleViolation,
                "Cannot approve the plan, the truck has not arrived at the dock");
        }

        ApprovedByUserId = teamLeadId;
        ApprovedByName = teamLeadName;
        Status = ManifestStatus.Loading;
        return Result.Success();
    }
    
    
    /// <summary>
    /// Rejects the plan with a comment for the planner and sends the manifest back to planning.
    /// Clears who confirmed the plan, so the planner has to confirm it again.
    /// Fails with a Conflict if the manifest is not awaiting confirmation.
    /// </summary>
    /// <param name="teamLeadId">The id of the team lead who rejects the plan.</param>
    /// <param name="teamLeadName">The name of the team lead who rejects the plan.</param>
    /// <param name="comment">What the planner has to fix.</param>
    /// <param name="now">The current time in UTC, passed in by the caller so tests can choose it.</param>
    /// <returns>Success, or a Conflict failure with the manifest's current status.</returns>
    /// <exception cref="ArgumentException">When the team lead id is empty,
    /// or the name or comment is blank.</exception>
    public Result Reject(Guid teamLeadId, string teamLeadName,
        string comment, DateTimeOffset now)
    {
        var rejection = new Rejection(teamLeadId, teamLeadName, comment, now);

        if (Status != ManifestStatus.AwaitingConfirmation)
        {
            return Result.Failure(ErrorType.Conflict,
                $"Cannot reject the plan, it is {Status}");
        }
        
        _rejections.Add(rejection);
        
        ConfirmedByUserId = null;
        ConfirmedByName = null;
        Status = ManifestStatus.UnderPlanning;
        return Result.Success();
    }
    
    /// <summary>
    /// Marks the truck as dispatched and records when it left the dock.
    /// Fails with a Conflict if the manifest is not being loaded.
    /// </summary>
    /// <param name="now">The current time in UTC, passed in by the caller so tests can choose it.</param>
    /// <returns>Success, or a Conflict failure with the manifest's current status.</returns>
    public Result Dispatch(DateTimeOffset now)
    {
        if (Status != ManifestStatus.Loading)
        {
            return Result.Failure(ErrorType.Conflict,
                $"Cannot dispatch the manifest, it is {Status}");
        }

        DispatchedAt = now;
        Status = ManifestStatus.Dispatched;
        return Result.Success();
    }
} 