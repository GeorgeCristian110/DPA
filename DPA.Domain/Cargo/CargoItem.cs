using DPA.Domain.Shared;

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

    /// <summary>
    /// Moves the pallet from storage to the dock and records when it arrived.
    /// Fails with a Conflict if the pallet is not in storage (already on the dock or expired) 
    /// </summary>
    /// <param name="now">The current time in UTC, passed in by the caller so tests can choose it.</param>
    /// <returns>Success, or a Conflict failure with the pallet's current status.</returns>
    public Result MoveToDock(DateTimeOffset now)
    {
        if (Status != CargoStatus.InStorage)
        {
            return Result.Failure(ErrorType.Conflict,
                $"Cargo {PalletCode} cannot be moved to the doc, it is {Status}");
        }
        
        Status = CargoStatus.OnDock;
        ArrivedOnDockAt = now;
        
        return Result.Success();
    }

    /// <summary>
    /// Assigns a pallet to a manifest, checks if the manifest id is empty and throws an exception.
    /// If the status of the pallet is expired after assigning a manifest changes it to on dock.
    /// </summary>
    /// <param name="manifestId">Shows to which manifest the pallet belongs to now.</param>
    /// <exception cref="ArgumentException">Throws the moment a manifest id is empty</exception>
    public void AssignToManifest(Guid manifestId)
    {
        if (manifestId == Guid.Empty)
        {
            throw new ArgumentException("The manifest id cannot be empty", nameof(manifestId));
        }

        if (Status == CargoStatus.Expired)
        {
            Status = CargoStatus.OnDock;
        }

        ManifestId = manifestId;
    }

    /// <summary>
    /// Marks the pallet as expired if it is on the dock, has no manifest and has been there
    /// longer than maxTimeOnDock.
    /// </summary>
    /// <param name="now">The current time in UTC, passed in by the caller so tests can choose it</param>
    /// <param name="maxTimeOnDock">Represents the max time a pallet can be on dock before it expires</param>
    /// <returns>Returns true if the pallet status has changed to expired,
    /// and returns false in case one of the conditions are true</returns>
    public bool MarkExpiredIfOverdue(DateTimeOffset now, TimeSpan maxTimeOnDock)
    {
        if (Status != CargoStatus.OnDock || ManifestId is not null
                                         || ArrivedOnDockAt is null
                                         || now - ArrivedOnDockAt.Value <= maxTimeOnDock)
        {
            return false;
        }
        
        Status = CargoStatus.Expired;
        return true;
    }

    /// <summary>
    /// Sends the pallet back to storage and clears its dock arrival time.
    /// Fails with a Conflict if the pallet is already in storage.
    /// </summary>
    /// <returns>Returns success if the pallet can be returned or failure if it is already there</returns>
    public Result ReturnToStorage()
    {
        if (Status == CargoStatus.InStorage)
        {
            return Result.Failure(ErrorType.Conflict,
                $"The pallet {PalletCode} can not be returned to storage it is {Status}");
        }

        Status = CargoStatus.InStorage;
        ArrivedOnDockAt = null;
        
        return Result.Success();
    }
}