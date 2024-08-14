using System;
using System.Collections;

namespace Jelly.GameContent;

public abstract class AbstractRegistry : IEnumerable
{
    public bool Initialized { get; private set; }

    internal protected void CheckInitialized()
    {
        if(Initialized)
            throw new InvalidOperationException("Unable to modify contents of the registry because it is locked");
    }

    internal void DoInit()
    {
        Init();
        Initialized = true;
    }

    /// <summary>
    /// When this method is overridden, it is used to register all entries at startup.
    /// </summary>
    public abstract void Init();

    public abstract IEnumerator GetEnumerator();
}
