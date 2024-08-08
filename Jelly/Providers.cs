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

    /// <summary>
    /// Use this method to set up the main providers at program start.
    /// </summary>
    public static void Initialize(NetworkProvider networkProvider)
    {
        System.ArgumentNullException.ThrowIfNull(networkProvider, nameof(networkProvider));

        if(_initialized) throw new System.InvalidOperationException(nameof(Initialize) + " cannot be called more than once.");
        _initialized = true;

        _networkProvider = networkProvider;
    }

    private static void CheckInitialized()
    {
        if(!_initialized)
        {
            throw new System.InvalidOperationException(nameof(Initialize) + " has not been called yet!");
        }
    }
}
