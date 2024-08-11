using System;
using System.Collections;

namespace Jelly.GameContent;

public abstract class AbstractRegistry
{
    public bool Locked { get; private set; }

    internal void CheckLocked()
    {
        if(Locked)
            throw new InvalidOperationException("Unable to modify contents of the registry because it is locked");
    }

    internal void DoInit()
    {
        Init();
        Locked = true;
    }

    /// <summary>
    /// When this method is overridden, it is used to register all entries at startup.
    /// </summary>
    public abstract void Init();
}
