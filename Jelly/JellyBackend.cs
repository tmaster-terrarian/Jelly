using System;

using Jelly.GameContent;

namespace Jelly;

public static class JellyBackend
{
    private static bool _initialized;

    internal static Random IDRandom = new();

    private static ContentProvider _contentProvider;

    public static ContentProvider ContentProvider
    {
        get {
            CheckInitialized();
            return _contentProvider;
        }
    }

    /// <summary>
    /// This method should called before calling any other update methods related to <see cref="Jelly"/>
    /// </summary>
    /// <param name="deltaTimeThisFrame">The current <i>unscaled</i> (real time) time in seconds since the last frame</param>
    public static void PreUpdate(float deltaTimeThisFrame)
    {
        Time.SetDeltaTime(deltaTimeThisFrame);
    }

    /// <summary>
    /// Sets up the main providers and engine backend utilities. Call once at program start.
    /// </summary>
    public static void Initialize(ContentProvider contentProvider)
    {
        ArgumentNullException.ThrowIfNull(contentProvider, nameof(contentProvider));

        if(_initialized) throw new InvalidOperationException(nameof(Initialize) + " cannot be called more than once!");
        _initialized = true;

        _contentProvider = contentProvider;

        Registries.Init();
    }

    private static void CheckInitialized()
    {
        if(!_initialized)
        {
            throw new InvalidOperationException(nameof(Initialize) + " has not been called yet!");
        }
    }
}
