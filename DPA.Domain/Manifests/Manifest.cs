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
    
}