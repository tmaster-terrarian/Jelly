namespace Jelly.Net;

internal interface INetworkedObject : INetID
{
    internal byte[] GetSyncPacket();

    internal void ReadSyncPacket(byte[] data);
}
