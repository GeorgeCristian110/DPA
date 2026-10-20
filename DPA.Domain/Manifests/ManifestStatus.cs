namespace DPA.Domain.Manifests;

public enum ManifestStatus
{
    /// <summary>
    /// The order is known, but the pallets are still in the warehouse.
    /// </summary>
    AwaitingPallets,
    
    /// <summary>
    /// Waiting for the planner to start drafting the loading plan.
    /// </summary>
    AwaitingPlanning,
    
    /// <summary>
    /// The planner has begun organizing the load structure.
    /// </summary>
    UnderPlanning,
    
    /// <summary>
    /// The planning is done and is sent to loading awaiting approval of team lead.
    /// </summary>
    SentToLoading,
    
    /// <summary>
    /// The loading process has started according to the submitted plan.
    /// </summary>
    Loading,
    
    /// <summary>
    /// The truck is loaded with all the manifest pallets and closed. It has left for the destination.
    /// </summary>
    Dispatched
}