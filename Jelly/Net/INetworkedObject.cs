namespace Jelly.Net;

public interface INetworkedObject
{
    public int NetID { get; }

    public byte[] GetSyncPacket();

    public void ReadSyncPacket(byte[] data);
}
