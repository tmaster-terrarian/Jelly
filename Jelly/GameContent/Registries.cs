using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Jelly.GameContent;

public static class Registries
{
    private static readonly HashSet<AbstractRegistry> _registries = [];
    private static bool _isDirty = true;
    private static bool initialized;

    private static AbstractRegistry[] _registriesAsArray;

    private static AbstractRegistry[] GetRegistriesAsArray()
    {
        if (_isDirty)
        {
            _registriesAsArray = new AbstractRegistry[_registries.Count];
            _registries.CopyTo(_registriesAsArray);

            _isDirty = false;
        }

        return _registriesAsArray;
    }

    internal static void Init()
    {
        CheckInitialized();
        initialized = true;

        foreach(var registry in _registries)
        {
            registry.DoInit();
        }
    }

    private static bool CheckInitialized()
    {
        if(initialized)
            throw new InvalidOperationException("Cannot add or remove registries during or after initialization");
        return true;
    }

    public static bool Add<T>([NotNullWhen(true)] [MaybeNullWhen(false)] Registry<T> registry) where T : ContentDef
    {
        return CheckInitialized() && (_isDirty |= _registries.Add(registry));
    }

    public static bool Add<T>([NotNullWhen(true)] [MaybeNullWhen(false)] Registry<T> registry, out int index) where T : ContentDef
    {
        CheckInitialized();

        _isDirty |= _registries.Add(registry);

        List<AbstractRegistry> _list = [..GetRegistriesAsArray()];
        index = _list.IndexOf(registry);

        return index != -1;
    }

    public static bool Remove<T>([NotNullWhen(true)] [MaybeNullWhen(false)] Registry<T> registry) where T : ContentDef
    {
        return CheckInitialized() && (_isDirty |= _registries.Remove(registry));
    }

    public static bool Remove<T>([NotNullWhen(true)] [MaybeNullWhen(false)] Registry<T> registry, out int index) where T : ContentDef
    {
        CheckInitialized();

        List<AbstractRegistry> _list = [..GetRegistriesAsArray()];
        index = _list.IndexOf(registry);

        return _isDirty |= _registries.Remove(registry);
    }

    public static Registry<T> GetAt<T>(int index) where T : ContentDef
    {
        var value = GetRegistriesAsArray()[index];
        return (value is Registry<T> registry) ? registry : null;
    }

    public static T Get<T>() where T : AbstractRegistry
    {
        foreach(var registry in _registries)
            if(registry is T reg)
                return reg;

        return null;
    }

    public static bool IsRegistered<T>([NotNullWhen(true)] [MaybeNullWhen(false)] Registry<T> registry) where T : ContentDef
    {
        return _registries.Contains(registry);
    }
}
