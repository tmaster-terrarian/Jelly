// Modified from: https://github.com/JamesMcMahon/monocle-engine/blob/master/Monocle/InternalUtilities/ComponentList.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Xml;

using Jelly.Net;
using Jelly.Serialization;

namespace Jelly.Utilities;

public class ComponentList : ICollection<Component>, IEnumerable<Component>, IEnumerable
{
    [JsonConverter(typeof(JsonStringEnumConverter<LockModes>))]
    public enum LockModes { Open, Locked, Error };

    [JsonIgnore] public Entity Entity { get; internal set; }

    private List<Component> Components { get; set; } = [];

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
                        Components.Add(component);
                        component.Added(Entity);

                        if(component.skipSync)
                        {
                            component.skipSync = false;
                            continue;
                        }

                        if((Entity?.CanUpdateLocally ?? false) && Entity?.Scene is not null)
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
                        Components.Remove(component);
                        component.Removed(Entity);

                        if(component.skipSync)
                        {
                            component.skipSync = false;
                            continue;
                        }

                        if((Entity?.CanUpdateLocally ?? false) && Entity?.Scene is not null)
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
                    Components.Add(component);
                    component.Added(Entity);

                    if(component.skipSync)
                    {
                        component.skipSync = false;
                        break;
                    }

                    if((Entity?.CanUpdateLocally ?? false) && Entity?.Scene is not null)
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

    public bool Remove(Component component)
    {
        bool result = false;
        switch (lockMode)
        {
            case LockModes.Open:
                if(current.Remove(component))
                {
                    Components.Remove(component);
                    component.Removed(Entity);
                    result = true;

                    if(component.skipSync)
                    {
                        component.skipSync = false;
                        break;
                    }

                    if((Entity?.CanUpdateLocally ?? false) && Entity?.Scene is not null)
                        Providers.NetworkProvider.SendSyncPacket(SyncPacketType.ComponentRemoved, component.GetInternalSyncPacket(), true);
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
        Remove(Components);
    }

    public int Count => Components.Count;

    public bool IsReadOnly { get; }

    public Component this[int index]
    {
        get
        {
            if (index < 0 || index >= Components.Count)
                throw new IndexOutOfRangeException();
            else
                return Components[index];
        }
    }

    public IEnumerator<Component> GetEnumerator()
    {
        return Components.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Component[] ToArray()
    {
        return [.. Components];
    }

    internal void Update()
    {
        LockMode = LockModes.Locked;
        foreach(var component in Components)
            if(component.Enabled)
                component.Update();
        LockMode = LockModes.Open;
    }

    internal void PreDraw()
    {
        LockMode = LockModes.Error;
        foreach(var component in Components)
            if(component.Visible)
                component.PreDraw();
        LockMode = LockModes.Open;
    }

    internal void Draw()
    {
        LockMode = LockModes.Error;
        foreach(var component in Components)
            if(component.Visible)
                component.Draw();
        LockMode = LockModes.Open;
    }

    internal void PostDraw()
    {
        LockMode = LockModes.Error;
        foreach(var component in Components)
            if(component.Visible)
                component.PostDraw();
        LockMode = LockModes.Open;
    }

    internal void DrawUI()
    {
        LockMode = LockModes.Error;
        foreach(var component in Components)
            if(component.Visible)
                component.DrawUI();
        LockMode = LockModes.Open;
    }

    public T Get<T>() where T : Component
    {
        foreach (var component in Components)
            if (component is T)
                return component as T;
        return null;
    }

    public Component FindByID(long id)
    {
        foreach (var component in Components)
            if (component.ComponentID == id)
                return component;
        return null;
    }

    public IEnumerable<T> GetAll<T>() where T : Component
    {
        foreach (var component in Components)
            if (component is T)
                yield return component as T;
    }

    internal void SendPackets()
    {
        for(int i = 0; i < Components.Count; i++)
        {
            Component component = Components[i];
            if(component.Enabled && component.SyncThisStep)
            {
                Providers.NetworkProvider.SendSyncPacket(SyncPacketType.ComponentUpdate, component.GetInternalSyncPacket(), component.SyncImportant);

                component.SyncThisStep = false;
                component.SyncImportant = false;
            }
        }
    }

    internal void ReadPacket(byte[] data)
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
        else
        {
            component.enabled = binReader.ReadBoolean();
            component.Visible = binReader.ReadBoolean();
        }

        component?.ReadPacket(data[(int)stream.Position..]);
    }

    internal void ReadNewComponentPacket(byte[] data)
    {
        using var stream = new MemoryStream(data);
        var binReader = new BinaryReader(stream);

        long id = binReader.ReadInt64();

        var typeName = binReader.ReadString();

        var component = FindByID(id);
        if(component is not null)
        {
            Logger.JellyLogger.Warn("CONFLICT: received an EntityAdded packet for an entity that already exists, the local entity will be overwritten!");
            component.skipSync = true;
            Remove(component);
        }

        component = Type.GetType(typeName).GetConstructor(Type.EmptyTypes).Invoke(null) as Component;
        component.skipSync = true;
        Add(component);

        component.enabled = binReader.ReadBoolean();
        component.Visible = binReader.ReadBoolean();

        component?.ReadPacket(data[(int)stream.Position..]);
    }

    internal void ReadRemovalPacket(byte[] data)
    {
        using var stream = new MemoryStream(data);
        var binReader = new BinaryReader(stream);

        long id = binReader.ReadInt64();

        var component = FindByID(id);
        if(component is null)
        {
            Logger.JellyLogger.Warn("DESYNC: received a packet for a component that doesn't exist!");
            return;
        }

        component.skipSync = true;

        Remove(component);
    }

    public bool Contains(Component item)
    {
        return Components.Contains(item);
    }

    void ICollection<Component>.CopyTo(Component[] array, int arrayIndex)
    {
        Components.CopyTo(array, arrayIndex);
    }
}
