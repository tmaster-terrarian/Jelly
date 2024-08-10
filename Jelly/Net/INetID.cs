namespace Jelly.Net;

internal interface INetID
{
    /// <summary>
    /// Corresponds to the index of the player that owns this entity.
    /// </summary>
    public int NetID { get; }
}
