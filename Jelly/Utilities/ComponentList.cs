// Modified from: https://github.com/JamesMcMahon/monocle-engine/blob/master/Monocle/InternalUtilities/ComponentList.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Serialization;
using Jelly.Components.Attributes;

namespace Jelly.Utilities;

public class ComponentList : ICollection<Component>, IEnumerable<Component>, IEnumerable
{
    [JsonConverter(typeof(JsonStringEnumConverter<LockModes>))]
    public enum LockModes { Open, Locked, Error };

    [JsonIgnore] public Entity Entity { get; internal set; }

    private readonly List<Component> components = [];
    private readonly List<Component> toAdd = [];
    private readonly List<Component> toRemove = [];

    private readonly HashSet<Component> current = new HashSet<Component>(Component.GetEqualityComparer());
    private readonly HashSet<Component> adding = new HashSet<Component>(Component.GetEqualityComparer());
    private readonly HashSet<Component> removing = new HashSet<Component>(Component.GetEqualityComparer());

    private LockModes lockMode;

    internal ComponentList(Entity entity)
    {
        Entity = entity;
    }

    public LockModes LockMode
    {
        get => lockMode;

        internal set
        {
            lockMode = value;

            if (toAdd.Count > 0)
            {
                foreach (var component in toAdd)
                {
                    if (current.Add(component))
                    {
                        components.Add(component);
                        component.Added(Entity);
                    }
                }

                adding.Clear();
                toAdd.Clear();
            }

            if (toRemove.Count > 0)
            {
                foreach (var component in toRemove)
                {
                    if (current.Remove(component))
                    {
                        components.Remove(component);
                        component.Removed(Entity);
                    }
                }

                removing.Clear();
                toRemove.Clear();
            }
        }
    }

    public void Add(Component component)
    {
        switch (LockMode)
        {
            case LockModes.Open:
                if(!Internal_ResolveComponentAttributes_PreAdd(component)) break;

                if(current.Add(component))
                {
                    components.Add(component);
                    component.Added(Entity);
                }
                break;

            case LockModes.Locked:
                if(!Internal_ResolveComponentAttributes_PreAdd(component)) break;

                if (!current.Contains(component) && !adding.Contains(component))
                {
                    adding.Add(component);
                    toAdd.Add(component);
                }
                break;

            case LockModes.Error:
                throw new Exception("Cannot add or remove Entities at this time!");
        }
    }

    public bool Remove(Component component)
    {
        bool result = false;
        switch (LockMode)
        {
            case LockModes.Open:
                if(current.Remove(component))
                {
                    components.Remove(component);
                    component.Removed(Entity);

                    Internal_ResolveComponentAttributes_PostRemove(component);

                    result = true;
                }
                break;

            case LockModes.Locked:
                if(current.Contains(component) && !removing.Contains(component))
                {
                    removing.Add(component);
                    toRemove.Add(component);

                    Internal_ResolveComponentAttributes_PostRemove(component);

                    result = true;
                }
                break;

            case LockModes.Error:
                throw new Exception("Cannot add or remove Entities at this time!");
        }
        return result;
    }

    public void Add(IEnumerable<Component> components)
    {
        foreach (var component in components)
            Add(component);
    }

    public bool Remove(IEnumerable<Component> components)
    {
        bool result = false;
        foreach (var component in components)
            result |= Remove(component);
        return result;
    }

    public void Add(params Component[] components)
    {
        foreach (var component in components)
            Add(component);
    }

    public bool Remove(params Component[] components)
    {
        bool result = false;
        foreach (var component in components)
            result |= Remove(component);
        return result;
    }

    public bool RemoveAll<T>() where T : Component
    {
        return Remove(GetAll<T>());
    }

    public void Clear()
    {
        Remove(components);
    }

    internal void Internal_ResolveComponentAttributes_PostRemove(Component component)
    {
        if(component is null) return;

        var t = component.GetType();

        Component[] components = [..this.components];
        foreach(var c in components)
        {
            var cType = c.GetType();

            if(cType.GetCustomAttributes<RequiredComponentAttribute>(inherit: true)
                is IEnumerable<RequiredComponentAttribute> dependencyAttributes)
            {
                foreach(var a in dependencyAttributes)
                {
                    if(a.ComponentType != t && !t.IsSubclassOf(a.ComponentType))
                        continue;

                    if(Get(a.ComponentType) is null)
                    {
                        Remove(c);
                        break;
                    }
                }
            }
        }
    }

    internal bool Internal_ResolveComponentAttributes_PreAdd(Component component)
    {
        if(component is null) return false;

        var t = component.GetType();

        if(t.GetCustomAttribute<SingletonComponentAttribute>(inherit: true)
            is SingletonComponentAttribute singletonAttribute)
        {
            if(Get(t) is not null)
                return false;
        }

        if(t.GetCustomAttributes<RequiredComponentAttribute>(inherit: true)
            is IEnumerable<RequiredComponentAttribute> dependencyAttributes)
        {
            foreach(var a in dependencyAttributes)
            {
                if(Get(a.ComponentType) is null)
                    return false;
            }
        }

        foreach(var c in components)
        {
            var cType = c.GetType();

            var eMsg =
                $"Entity {Entity.EntityID} is about to add a component with the type {t.FullName}, which is mutually exclusive or incompatible with {cType.FullName}!";

            if(cType.GetCustomAttributes<MutuallyExclusiveComponentAttribute>(inherit: true)
                is IEnumerable<MutuallyExclusiveComponentAttribute> cExclusiveAttributes)
            {
                List<Type> types = [];
                List<MutuallyExclusiveComponentKind> kinds = [];

                foreach(var a in cExclusiveAttributes)
                {
                    types.Add(a.ComponentType);
                    kinds.Add(a.ExclusionKind);
                }

                int i = types.IndexOf(t);
                if(i != -1)
                {
                    switch(kinds[i])
                    {
                        case MutuallyExclusiveComponentKind.Default:
                            return false;

                        case MutuallyExclusiveComponentKind.Warn:
                            JellyBackend.Logger.LogWarning(eMsg);
                            break;

                        case MutuallyExclusiveComponentKind.Throw:
                            throw new InvalidOperationException(eMsg);

                        default:
                            throw new InvalidOperationException(
                                $"The value {(int)kinds[i]} is not a valid value of {nameof(MutuallyExclusiveComponentKind)}");
                    }
                }
            }
        }

        if(t.GetCustomAttributes<MutuallyExclusiveComponentAttribute>(inherit: true)
            is IEnumerable<MutuallyExclusiveComponentAttribute> exclusiveAttributes)
        {
            List<Type> types = [];
            List<MutuallyExclusiveComponentKind> kinds = [];

            foreach(var a in exclusiveAttributes)
            {
                types.Add(a.ComponentType);
                kinds.Add(a.ExclusionKind);
            }

            Component[] components = [..this.components];
            foreach(var c in components)
            {
                var cType = c.GetType();

                var eMsg =
                    $"Entity {Entity.EntityID} has one or more components with the type {cType.FullName}, which is mutually exclusive or incompatible with {t.FullName}!";

                int i = types.IndexOf(cType);
                if(i != -1)
                {
                    switch(kinds[i])
                    {
                        case MutuallyExclusiveComponentKind.Default:
                            Remove(c);
                            break;

                        case MutuallyExclusiveComponentKind.Warn:
                            JellyBackend.Logger.LogWarning(eMsg);
                            break;

                        case MutuallyExclusiveComponentKind.Throw:
                            throw new InvalidOperationException(eMsg);

                        default:
                            throw new InvalidOperationException(
                                $"The value {(int)kinds[i]} is not a valid value of {nameof(MutuallyExclusiveComponentKind)}");
                    }
                }
            }
        }

        return true;
    }

    public int Count => components.Count;

    public bool IsReadOnly { get; }

    public Component this[int index] {
        get {
            if (index < 0 || index >= components.Count)
                throw new IndexOutOfRangeException();

            return components[index];
        }
    }

    public IEnumerator<Component> GetEnumerator()
    {
        return components.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Component[] ToArray()
    {
        return [.. components];
    }

    internal void Update()
    {
        LockMode = LockModes.Locked;
        foreach(var component in components)
            if(component.Enabled)
                component.Update();
        LockMode = LockModes.Open;
    }

    internal void PreDraw()
    {
        LockMode = LockModes.Error;
        foreach(var component in components)
            if(component.Visible)
                component.PreDraw();
        LockMode = LockModes.Open;
    }

    internal void Draw()
    {
        LockMode = LockModes.Error;
        foreach(var component in components)
            if(component.Visible)
                component.Draw();
        LockMode = LockModes.Open;
    }

    internal void PostDraw()
    {
        LockMode = LockModes.Error;
        foreach(var component in components)
            if(component.Visible)
                component.PostDraw();
        LockMode = LockModes.Open;
    }

    internal void DrawUI()
    {
        LockMode = LockModes.Error;
        foreach(var component in components)
            if(component.Visible)
                component.DrawUI();
        LockMode = LockModes.Open;
    }

    public T? Get<T>() where T : Component
    {
        foreach (var component in components)
            if (component is T)
                return component as T;
        return null;
    }

    public IEnumerable<T> GetAll<T>() where T : Component
    {
        foreach (var component in components)
            if (component is T)
                yield return component as T;
    }

    public Component? Get(Type type)
    {
        if(!(type.IsClass && type.IsSubclassOf(typeof(Component))))
            throw new InvalidCastException($"{nameof(type)} must derive from the type {nameof(Component)}");

        foreach (var component in components)
            if (component.GetType().IsSubclassOf(type) || component.GetType() == type)
                return component;
        return null;
    }

    public IEnumerable<Component> GetAll(Type type)
    {
        if(!(type.IsClass && type.IsSubclassOf(typeof(Component))))
            throw new InvalidCastException($"{nameof(type)} must derive from the type {nameof(Component)}");

        foreach (var component in components)
            if (component.GetType().IsSubclassOf(type) || component.GetType() == type)
                yield return component;
    }

    public bool Contains(Component item)
    {
        return components.Contains(item);
    }

    void ICollection<Component>.CopyTo(Component[] array, int arrayIndex)
    {
        components.CopyTo(array, arrayIndex);
    }
}
