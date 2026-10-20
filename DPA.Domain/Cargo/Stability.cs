namespace DPA.Domain.Cargo;

public enum Stability
{
    /// <summary>
    /// A pallet that can be loaded normally and can stand on its own and can
    /// support pallets next to it.
    /// </summary>
    Stable,
    
    /// <summary>
    /// A pallet that when loaded needs to be in the middle or receive support from other pallets.
    /// </summary>
    Unstable
}