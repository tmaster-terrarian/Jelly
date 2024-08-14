using System.Collections;
using System.Collections.Generic;

namespace Jelly.GameContent;

public abstract class Registry<TDef> : AbstractRegistry, IEnumerable<KeyValuePair<string, TDef>> where TDef : ContentDef
{
    private readonly Dictionary<string, TDef> registeredDefs = [];

    public TDef this[string key]
    {
        get => GetDef(key) ?? throw new KeyNotFoundException();
    }

    public TDef? GetDef(string key)
    {
        if(registeredDefs.TryGetValue(key, out TDef value))
            return value;

        return null;
    }

    public static TDef? GetDefStatic(string name) => Registries.FindFirst<Registry<TDef>>()?.GetDef(name);

    public bool Register(TDef value)
    {
        CheckInitialized();

        System.ArgumentNullException.ThrowIfNull(value, nameof(value));

        return Register(value, value.Name);
    }

    public bool Register(TDef value, string key)
    {
        CheckInitialized();

        System.ArgumentNullException.ThrowIfNull(value, nameof(value));
        System.ArgumentNullException.ThrowIfNull(key, nameof(key));

        // if(value.Name != key) value.Name = key;

        if(!Initialized && registeredDefs.TryAdd(key, value))
        {
            EntryAdded?.Invoke(value, key);
            return true;
        }

        return false;
    }

    public bool UnRegister(string key)
    {
        CheckInitialized();

        System.ArgumentNullException.ThrowIfNull(key, nameof(key));

        if(!Initialized && registeredDefs.Remove(key, out TDef value))
        {
            EntryRemoved?.Invoke(value, key);
            return true;
        }

        return false;
    }

    public bool UnRegister(TDef value)
    {
        CheckInitialized();

        System.ArgumentNullException.ThrowIfNull(value, nameof(value));

        if(!Initialized && registeredDefs.Remove(value.Name, out TDef v) && ReferenceEquals(value, v))
        {
            EntryRemoved?.Invoke(v, v.Name);
            return true;
        }

        return false;
    }

    public event EntryEventDelegate EntryAdded;

    public event EntryEventDelegate EntryRemoved;

    public override IEnumerator<KeyValuePair<string, TDef>> GetEnumerator()
    {
        foreach(var def in registeredDefs)
        {
            yield return def;
        }
    }

    public virtual IReadOnlyCollection<string> Keys => registeredDefs.Keys;

    public virtual IReadOnlyCollection<TDef> Values => registeredDefs.Values;

    public delegate void EntryEventDelegate(TDef? value, string key);
}
