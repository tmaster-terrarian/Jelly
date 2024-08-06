using System;

namespace Jelly;

public class ObjectDisposedEventArgs(bool wasDisposedByGC) : EventArgs
{
    public bool DisposedByGC { get; } = wasDisposedByGC;
}
