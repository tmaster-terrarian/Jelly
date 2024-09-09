// Modified from: https://github.com/JamesMcMahon/monocle-engine/blob/master/Monocle/InternalUtilities/ComponentList.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Jelly.Utilities;

public class ComponentList : ICollection<Component>, IEnumerable<Component>, IEnumerable
{
    [JsonConverter(typeof(JsonStringEnumConverter<LockModes>))]
    public enum LockModes { Open, Locked, Error };

    [JsonIgnore] public Entity Entity { get; internal set; }

    private readonly List<Component> components = [];
    private readonly List<Component> toAdd = [];
    private readonly List<Component> toRemove = [];

    private readonly HashSet<Component> current = [];
    private readonly HashSet<Component> adding = [];
    private readonly HashSet<Component> removing = [];

    private LockModes lockMode;

    internal ComponentList(Entity entity)
    {
        Entity = entity;
    }

    internal LockModes LockMode
    {
        get
        {
            return lockMode;
        }

        set
        {
            lockMode = value;

            if (toAdd.Count > 0)
            {
                foreach (var component in toAdd)
                {
                    if(current.Add(component))
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
                    if(current.Remove(component))
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
        switch (lockMode)
        {
            case LockModes.Open:
                if(current.Add(component))
                {
                    components.Add(component);
                    component.Added(Entity);
                }
                break;

            case LockModes.Locked:
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
        switch (lockMode)
        {
            case LockModes.Open:
                if(current.Remove(component))
                {
                    components.Remove(component);
                    component.Removed(Entity);
                    result = true;
                }
                break;

            case LockModes.Locked:
                if (current.Contains(component) && !removing.Contains(component))
                {
                    removing.Add(component);
                    toRemove.Add(component);
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

    public int Count => components.Count;

    public bool IsReadOnly { get; }

    public Component this[int index]
    {
        get
        {
            if (index < 0 || index >= components.Count)
                throw new IndexOutOfRangeException();
            else
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

    public bool Contains(Component item)
    {
        return components.Contains(item);
    }

    void ICollection<Component>.CopyTo(Component[] array, int arrayIndex)
    {
        components.CopyTo(array, arrayIndex);
    }
}
