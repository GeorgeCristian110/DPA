namespace DPA.Domain.Manifests;

public class Rejection
{
    public Guid Id { get; private set; }
    public Guid RejectedByUserId { get; private set; }
    public string RejectedByUserName { get; private set; }
    public string RejectionMessage { get; private set; }
    public DateTimeOffset RejectionDate { get; private set; }
    

    internal Rejection(Guid rejectedByUserId, string rejectedByUserName, 
        string rejectionMessage,  DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rejectedByUserName);
        ArgumentException.ThrowIfNullOrWhiteSpace(rejectionMessage);
        
        if (rejectedByUserId == Guid.Empty)
        {
            throw new ArgumentException("RejectedByUserId is required and can not be empty.",
                nameof(rejectedByUserId));
        }
        

        Id = Guid.NewGuid();
        RejectedByUserId = rejectedByUserId;
        RejectedByUserName = rejectedByUserName;
        RejectionMessage = rejectionMessage;
        RejectionDate = now;
    }
}