using System.Collections;
using System.Collections.Generic;

namespace Jelly.GameContent;

public abstract class Registry<TDef> : AbstractRegistry, IEnumerable, IEnumerable<Dictionary<string, TDef>.Enumerator> where TDef : ContentDef
{
    private readonly Dictionary<string, TDef> registeredDefs = [];

    public TDef? GetDef(string key)
    {
        if(registeredDefs.TryGetValue(key, out TDef value))
            return value;

        return null;
    }

    public static TDef? GetDefStatic(string name) => Registries.GetRegistry<Registry<TDef>>()?.GetDef(name);

    public bool Register(TDef value)
    {
        CheckLocked();

        return Register(value, value.Name);
    }

    public bool Register(TDef value, string key)
    {
        CheckLocked();

        if(!Locked && registeredDefs.TryAdd(key, value))
        {
            EntryAdded(value, key);
            return true;
        }

        return false;
    }

    public bool UnRegister(string key)
    {
        CheckLocked();

        if(!Locked && registeredDefs.Remove(key, out TDef value))
        {
            EntryRemoved(value, key);
            return true;
        }

        return false;
    }

    public bool UnRegister(TDef value)
    {
        CheckLocked();

        if(!Locked && registeredDefs.Remove(value.Name, out TDef v) && ReferenceEquals(value, v))
        {
            EntryRemoved(v, v.Name);
            return true;
        }

        return false;
    }

    protected virtual void EntryAdded(TDef? value, string key) {}

    protected virtual void EntryRemoved(TDef? value, string key) {}

    public IEnumerator<Dictionary<string, TDef>.Enumerator> GetEnumerator()
    {
        yield return registeredDefs.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
