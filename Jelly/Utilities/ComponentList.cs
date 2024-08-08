// Modified from: https://github.com/JamesMcMahon/monocle-engine/blob/master/Monocle/InternalUtilities/ComponentList.cs
using System;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Jelly.Components;

namespace Jelly.Utilities;

public class ComponentList : IEnumerable<Component>, IEnumerable
{
    public enum LockModes { Open, Locked, Error };
    public Entity Entity { get; internal set; }

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
                    if (!current.Contains(component))
                    {
                        current.Add(component);
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
                    if (current.Contains(component))
                    {
                        current.Remove(component);
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
                if (!current.Contains(component))
                {
                    current.Add(component);
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

    public void Remove(Component component)
    {
        switch (lockMode)
        {
            case LockModes.Open:
                if (current.Contains(component))
                {
                    current.Remove(component);
                    components.Remove(component);
                    component.Removed(Entity);
                }
                break;

            case LockModes.Locked:
                if (current.Contains(component) && !removing.Contains(component))
                {
                    removing.Add(component);
                    toRemove.Add(component);
                }
                break;

            case LockModes.Error:
                throw new Exception("Cannot add or remove Entities at this time!");
        }
    }

    public void Add(IEnumerable<Component> components)
    {
        foreach (var component in components)
            Add(component);
    }

    public void Remove(IEnumerable<Component> components)
    {
        foreach (var component in components)
            Remove(component);
    }

    public void RemoveAll<T>() where T : Component
    {
        Remove(GetAll<T>());
    }

    public void Add(params Component[] components)
    {
        foreach (var component in components)
            Add(component);
    }

    public void Remove(params Component[] components)
    {
        foreach (var component in components)
            Remove(component);
    }

    public int Count
    {
        get
        {
            return components.Count;
        }
    }

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

    IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Component[] ToArray()
    {
        return [.. components];
    }

    internal void Update(GameTime gameTime)
    {
        LockMode = LockModes.Locked;
        foreach (var component in components)
            if (component.Enabled)
                component.Update(gameTime);
        LockMode = LockModes.Open;
    }

    internal void Draw(GameTime gameTime)
    {
        LockMode = LockModes.Error;
        foreach (var component in components)
            if (component.Visible)
                component.Draw(gameTime);
        LockMode = LockModes.Open;
    }

    // internal void DebugRender(Camera camera)
    // {
    //     LockMode = LockModes.Error;
    //     foreach (var component in components)
    //         component.DebugRender(camera);
    //     LockMode = LockModes.Open;
    // }

    // internal void HandleGraphicsReset()
    // {
    //     LockMode = LockModes.Error;
    //     foreach (var component in components)
    //         component.HandleGraphicsReset();
    //     LockMode = LockModes.Open;
    // }

    // internal void HandleGraphicsCreate()
    // {
    //     LockMode = LockModes.Error;
    //     foreach (var component in components)
    //         component.HandleGraphicsCreate();
    //     LockMode = LockModes.Open;
    // }

    public T Get<T>() where T : Component
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
}
