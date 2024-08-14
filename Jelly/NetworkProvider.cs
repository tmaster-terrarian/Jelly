using Jelly.Net;

namespace Jelly;

public abstract class NetworkProvider
{
    /// <summary>
    /// This value is <see langword="true"/> if multiplayer networking is enabled, otherwise <see langword="false"/>
    /// </summary>
    public virtual bool NetworkingEnabled { get; }

    /// <summary>
    /// This value is <see langword="true"/> if this client is the owner of the lobby, otherwise <see langword="false"/>
    /// </summary>
    public virtual bool IsHost => GetHostNetID() == GetNetID();

    /// <summary>
    /// Override this method to provide the client's NetID (ie. the player's index in a lobby)
    /// </summary>
    /// <returns>The index of the client this process is in charge of</returns>
    public abstract int GetNetID();

    /// <summary>
    /// Override this method to provide the host's NetID (ie. the host's index in a lobby)
    /// </summary>
    /// <returns>The index of the client that started and manages the lobby</returns>
    public abstract int GetHostNetID();

    /// <summary>
    /// This method will be called for each entity that wants to update
    /// </summary>
    /// <param name="syncPacketType">The type of sync packet being sent</param>
    /// <param name="data">The data to send to other clients</param>
    /// <param name="important">Whether this packet should be sent in a reliable manner</param>
    public abstract void SendSyncPacket(SyncPacketType syncPacketType, byte[] data, bool important);

    internal event PacketEvent PacketReceived;

    internal delegate void PacketEvent(byte[] data, int senderNetID);

    /// <summary>
    /// Use this to raise the <see cref="PacketReceived"/> event whenever a multiplayer packet is received 
    /// </summary>
    public void RaisePacketReceivedEvent(byte[] data, int senderNetID)
    {
        PacketReceived?.Invoke(data, senderNetID);
    }
}
