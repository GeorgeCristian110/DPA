namespace DPA.Domain.Cargo;

public enum CargoStatus
{
    /// <summary>
    /// The pallets exist in the company storage and can be brought to the dock.
    /// </summary>
    InStorage,
    
    /// <summary>
    /// The pallets have arrived on the dock and are ready to be sent to loading.
    /// </summary>
    OnDock,
    
    /// <summary>
    /// A pallet has been standing too long without being assigned a manifest.
    /// The workers have to choose what to do with it.
    /// </summary>
    Expired
}