using System;

using Jelly.Net;

namespace Jelly;

public static class Providers
{
    private static bool _initialized;

    private static NetworkProvider _networkProvider;

    public static NetworkProvider NetworkProvider
    {
        get {
            CheckInitialized();
            return _networkProvider;
        }
    }

    private static float deltaTime;

    public static float DeltaTime => deltaTime;

    internal static event PacketEvent PacketReceived;

    internal delegate void PacketEvent(byte[] data, int senderNetID);

    /// <summary>
    /// Use this to raise the <see cref="PacketReceived"/> event whenever a multiplayer packet is received 
    /// </summary>
    public static void RaisePacketReceivedEvent(byte[] data)
    {
        PacketReceived?.Invoke(data, NetworkProvider.GetNetID());
    }

    /// <summary>
    /// This method should called before calling any other update methods related to <see cref="Jelly"/>
    /// </summary>
    public static void SetDeltaTime(float value)
    {
        deltaTime = value;
    }

    /// <summary>
    /// Use this method to set up the main providers at program start.
    /// </summary>
    public static void Initialize(NetworkProvider networkProvider)
    {
        ArgumentNullException.ThrowIfNull(networkProvider, nameof(networkProvider));

        if(_initialized) throw new InvalidOperationException(nameof(Initialize) + " cannot be called more than once.");
        _initialized = true;

        _networkProvider = networkProvider;
    }

    private static void CheckInitialized()
    {
        if(!_initialized)
        {
            throw new InvalidOperationException(nameof(Initialize) + " has not been called yet!");
        }
    }
}
