using System.Collections;
using System.Collections.Generic;

namespace Jelly.GameContent;

public abstract class Registry<T> : IEnumerable, IEnumerable<Dictionary<string, T>.Enumerator> where T : ContentDef
{
    private readonly Dictionary<string, T> registeredDefs = [];

    public T GetDef(string key)
    {
        if(registeredDefs.TryGetValue(key, out T value))
            return value;

        return null;
    }

    public T Register(string key, T value)
    {
        return registeredDefs.TryAdd(key, value) ? value : null;
    }

    public T Register(T value)
    {
        return registeredDefs.TryAdd(value.Name, value) ? value : null;
    }

    public bool UnRegister(string key)
    {
        return registeredDefs.Remove(key);
    }

    public IEnumerator<Dictionary<string, T>.Enumerator> GetEnumerator()
    {
        yield return registeredDefs.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
