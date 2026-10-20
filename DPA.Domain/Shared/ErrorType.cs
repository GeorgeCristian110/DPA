namespace DPA.Domain.Shared;

public enum ErrorType
{
    /// <summary>
    /// This result failure will be produced on for example if a
    /// request itself is incomplete or badly formatted, for example a missing pallet code
    /// </summary>
    Validation,
    
    /// <summary>
    /// A failure for the result, will happen when someone 
    /// tries to find a manifest with an id that does not exist.
    /// </summary>
    NotFound,
    
    /// <summary>
    /// A failure on result will happen if for example a pallet is already on dock.
    /// </summary>
    Conflict,
    
    /// <summary>
    /// This failure on result , can happen if the weight or height are valid but higher than the
    /// company business rules.
    /// </summary>
    RuleViolation
}