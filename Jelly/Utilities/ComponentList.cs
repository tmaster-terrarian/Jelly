// Modified from: https://github.com/JamesMcMahon/monocle-engine/blob/master/Monocle/InternalUtilities/ComponentList.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Jelly.Net;

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
                    if(current.Add(component))
                    {
                        components.Add(component);
                        component.Added(Entity);

                        if(component.skipSync)
                        {
                            component.skipSync = false;
                            continue;
                        }

                        if(Entity is not null)
                            Providers.NetworkProvider.SendSyncPacket(SyncPacketType.ComponentAdded, component.GetInternalSyncPacket(), true);
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

                        if(component.skipSync)
                        {
                            component.skipSync = false;
                            continue;
                        }

                        if(Entity is not null)
                            Providers.NetworkProvider.SendSyncPacket(SyncPacketType.ComponentRemoved, component.GetInternalSyncPacket(), true);
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

                    if(component.skipSync)
                    {
                        component.skipSync = false;
                        break;
                    }

                    if(Entity is not null)
                        Providers.NetworkProvider.SendSyncPacket(SyncPacketType.ComponentAdded, component.GetInternalSyncPacket(), true);
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
                if(current.Remove(component))
                {
                    components.Remove(component);
                    component.Removed(Entity);

                    if(component.skipSync)
                    {
                        component.skipSync = false;
                        break;
                    }

                    if(Entity is not null)
                        Providers.NetworkProvider.SendSyncPacket(SyncPacketType.ComponentRemoved, component.GetInternalSyncPacket(), true);
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

    public int Count => components.Count;

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

    public T Get<T>() where T : Component
    {
        foreach (var component in components)
            if (component is T)
                return component as T;
        return null;
    }

    internal Component FindByID(long id)
    {
        foreach (var component in components)
            if (component.ComponentID == id)
                return component;
        return null;
    }

    public IEnumerable<T> GetAll<T>() where T : Component
    {
        foreach (var component in components)
            if (component is T)
                yield return component as T;
    }

    internal void SendPackets()
    {
        for(int i = 0; i < components.Count; i++)
        {
            Component component = components[i];
            if (component.Enabled && component.SyncThisStep)
            {
                Providers.NetworkProvider.SendSyncPacket(SyncPacketType.ComponentUpdate, component.GetInternalSyncPacket(), component.SyncImportant);

                component.SyncThisStep = false;
                component.SyncImportant = false;
            }
        }
    }

    internal void ReadPacket(byte[] data, bool newComponent = false)
    {
        using var stream = new MemoryStream(data);
        var binReader = new BinaryReader(stream);

        long id = binReader.ReadInt64();

        var typeName = binReader.ReadString();

        var component = FindByID(id);
        if(component is null)
        {
            if(!newComponent)
            {
                Logger.JellyLogger.Error("DESYNC: received a packet for a component that doesn't exist!");
                return;
            }

            component = Type.GetType(typeName).GetConstructor(Type.EmptyTypes).Invoke(null) as Component;
            component.skipSync = true;
            Add(component);
        }

        component.enabled = binReader.ReadBoolean();
        component.Visible = binReader.ReadBoolean();

        component?.ReadPacket(data[(int)stream.Position..]);
    }

    internal void ReadRemovalPacket(byte[] data)
    {
        using var stream = new MemoryStream(data);
        var binReader = new BinaryReader(stream);

        long id = binReader.ReadInt64();

        binReader.ReadString();

        var component = FindByID(id);
        if(component is null)
        {
            Logger.JellyLogger.Error("DESYNC: received a packet for a component that doesn't exist!");
            return;
        }

        component.skipSync = true;

        Remove(component);
    }
}
