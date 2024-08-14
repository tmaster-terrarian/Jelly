// Modified from: https://github.com/JamesMcMahon/monocle-engine/blob/master/Monocle/InternalUtilities/ComponentList.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

using Microsoft.Xna.Framework;

using Jelly.Net;

namespace Jelly.Utilities;

public class EntityList : ICollection<Entity>, IEnumerable<Entity>, IEnumerable
{
    public static Comparison<Entity> CompareDepth => (a, b) => Math.Sign(b.Depth - a.Depth);

    private List<Entity> Entities { get; set; } = [];

    private readonly List<Entity> toAdd = [];
    private readonly List<Entity> toAwake = [];
    private readonly List<Entity> toRemove = [];

    private readonly HashSet<Entity> current = [];
    private readonly HashSet<Entity> adding = [];
    private readonly HashSet<Entity> removing = [];

    private bool unsorted;

    [JsonIgnore] public Scene Scene { get; internal set; }

    [JsonIgnore]
    public int Count => Entities.Count;

    public bool IsReadOnly { get; }

    [JsonIgnore]
    public Entity this[int index]
    {
        get
        {
            if (index < 0 || index >= Entities.Count)
                throw new IndexOutOfRangeException();
            else
                return Entities[index];
        }
    }

    internal EntityList(Scene scene)
    {
        Scene = scene;
    }

    internal void MarkUnsorted()
    {
        unsorted = true;
    }

    public void UpdateLists()
    {
        if (toAdd.Count > 0)
        {
            foreach (var entity in toAdd)
            {
                if(current.Add(entity))
                {
                    Entities.Add(entity);

                    if (Scene != null)
                    {
                        // Scene.TagLists.EntityAdded(entity);
                        // Scene.Tracker.EntityAdded(entity);

                        entity.Added(Scene);

                        if(entity.skipSync)
                        {
                            entity.skipSync = false;
                            continue;
                        }

                        if(!entity?.CanUpdateLocally ?? false)
                            continue;

                        Providers.NetworkProvider.SendSyncPacket(SyncPacketType.EntityAdded, entity.GetSyncPacket(), true);

                        if(entity.Components is not null)
                            foreach(var c in entity.Components)
                                Providers.NetworkProvider.SendSyncPacket(SyncPacketType.ComponentAdded, c.GetInternalSyncPacket(), true);
                    }
                }
            }

            unsorted = true;
        }

        if (toRemove.Count > 0)
        {
            foreach (var entity in toRemove)
            {
                if(Entities.Remove(entity))
                {
                    current.Remove(entity);

                    if(Scene != null)
                    {
                        entity.Removed(Scene);
                        // Scene.TagLists.EntityRemoved(entity);
                        // Scene.Tracker.EntityRemoved(entity);
                        // Engine.Pooler.EntityRemoved(entity);

                        if(entity.skipSync)
                        {
                            entity.skipSync = false;
                            continue;
                        }

                        if(!entity?.CanUpdateLocally ?? false)
                            continue;

                        Providers.NetworkProvider.SendSyncPacket(SyncPacketType.EntityRemoved, entity.GetSyncPacket(), true);
                    }
                }
            }

            toRemove.Clear();
            removing.Clear();
        }

        if (unsorted)
        {
            unsorted = false;
            Entities.Sort(CompareDepth);
        }

        if (toAdd.Count > 0)
        {
            toAwake.AddRange(toAdd);
            toAdd.Clear();
            adding.Clear();

            foreach (var entity in toAwake)
                if (entity.Scene == Scene)
                    entity.Awake(Scene);
            toAwake.Clear();
        }
    }

    public int IndexOf(Entity entity) => Entities.IndexOf(entity);

    public void Add(Entity entity)
    {
        if (!adding.Contains(entity) && !current.Contains(entity))
        {
            adding.Add(entity);
            toAdd.Add(entity);
        }
    }

    public bool Remove(Entity entity)
    {
        if(!removing.Contains(entity) && current.Contains(entity))
        {
            removing.Add(entity);
            toRemove.Add(entity);
            return true;
        }
        return false;
    }

    public void Add(IEnumerable<Entity> entities)
    {
        foreach (Entity entity in entities)
            Add(entity);
    }

    public bool Remove(IEnumerable<Entity> entities)
    {
        bool result = false;
        foreach (Entity entity in entities)
            result |= Remove(entity);
        return result;
    }

    public void Add(params Entity[] entities)
    {
        foreach (Entity entity in entities)
            Add(entity);
    }

    public bool Remove(params Entity[] entities)
    {
        bool result = false;
        foreach (Entity entity in entities)
            result |= Remove(entity);
        return result;
    }

    public int AmountOf<T>() where T : Entity
    {
        int count = 0;
        foreach (var e in Entities)
            if (e is T)
                count++;

        return count;
    }

    public T FindFirst<T>() where T : Entity
    {
        foreach (var e in Entities)
            if (e is T)
                return e as T;

        return null;
    }

    public Entity FindByID(long id)
    {
        foreach (var e in Entities)
            if (e.EntityID == id)
                return e;

        return null;
    }

    public List<T> FindAll<T>() where T : Entity
    {
        List<T> list = [];

        foreach (var e in Entities)
            if (e is T)
                list.Add(e as T);

        return list;
    }

    public void With<T>(Action<T> action) where T : Entity
    {
        foreach (var e in Entities)
            if (e is T)
                action(e as T);
    }

    public IEnumerator<Entity> GetEnumerator()
    {
        return Entities.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Entity[] ToArray()
    {
        return [.. Entities];
    }

    public bool HasVisibleEntities(int matchTags, TagFilter filter = TagFilter.AtLeastOne)
    {
        switch(filter)
        {
            case TagFilter.AtLeastOne:
                foreach(var entity in Entities)
                    if(entity.Visible && entity.TagIncludes(matchTags))
                        return true;
                break;

            case TagFilter.All:
                foreach(var entity in Entities)
                    if(entity.Visible && entity.TagMatches(matchTags))
                        return true;
                break;

            case TagFilter.None:
                foreach(var entity in Entities)
                    if(entity.Visible && !entity.TagIncludes(matchTags))
                        return true;
                break;
        }

        return false;
    }

    internal void Update()
    {
        foreach(var entity in Entities)
            if(entity.Enabled && entity.CanUpdateLocally)
                entity.Update();
    }

    private void Draw(int phase)
    {
        foreach(var entity in Entities)
            if(entity.Visible)
                DrawPhase(entity, phase);
    }

    private void Draw(int phase, int matchTags, TagFilter filter)
    {
        switch(filter)
        {
            case TagFilter.AtLeastOne:
                foreach(var entity in Entities)
                    if(entity.Visible && entity.TagIncludes(matchTags))
                        DrawPhase(entity, phase);
                break;

            case TagFilter.All:
                foreach(var entity in Entities)
                    if(entity.Visible && entity.TagMatches(matchTags))
                        DrawPhase(entity, phase);
                break;

            case TagFilter.None:
                foreach(var entity in Entities)
                    if(entity.Visible && !entity.TagIncludes(matchTags))
                        DrawPhase(entity, phase);
                break;
        }
    }

    private static void DrawPhase(Entity entity, int phase)
    {
        switch(phase)
        {
            case 0:
                entity.PreDraw();
                break;
            case 1:
                entity.Draw();
                break;
            case 2:
                entity.PostDraw();
                break;
            case 3:
                entity.DrawUI();
                break;
        }
    }

    internal void PreDraw() => Draw(0);

    internal void PreDraw(int matchTags, TagFilter filter) => Draw(0, matchTags, filter);

    internal void Draw() => Draw(1);

    internal void Draw(int matchTags, TagFilter filter) => Draw(1, matchTags, filter);

    internal void PostDraw() => Draw(2);

    internal void PostDraw(int matchTags, TagFilter filter) => Draw(2, matchTags, filter);

    internal void DrawUI() => Draw(3);

    internal void DrawUI(int matchTags, TagFilter filter) => Draw(3, matchTags, filter);

    internal void SendPackets()
    {
        if(Scene is null) return;

        for(int i = 0; i < Entities.Count; i++)
        {
            Entity entity = Entities[i];
            if(entity.CanUpdateLocally && entity.Enabled && entity.SyncThisStep)
            {
                Providers.NetworkProvider.SendSyncPacket(SyncPacketType.EntityUpdate, entity.GetSyncPacket(), entity.SyncImportant);

                entity.SyncThisStep = false;
                entity.SyncImportant = false;
            }
        }
    }

    internal void ReadPacket(byte[] data, int netId)
    {
        using var binReader = new BinaryReader(new MemoryStream(data));

        var sceneId = binReader.ReadInt64();
        if(Scene?.SceneID != sceneId)
            return;

        var id = binReader.ReadInt64();
        var entity = FindByID(id);

        if(entity is null)
        {
            Logger.JellyLogger.Error("CONFLICT: received a packet for an entity that doesn't exist, this will create a new one!");

            entity = new(Point.Zero, netId) {
                EntityID = id,
                skipSync = true
            };

            Add(entity);
        }

        entity.Enabled = binReader.ReadBoolean();
        entity.Visible = binReader.ReadBoolean();
        entity.Position = new(binReader.ReadInt32(), binReader.ReadInt32());
    }

    internal void ReadNewEntityPacket(byte[] data, int netId)
    {
        using var binReader = new BinaryReader(new MemoryStream(data));

        var sceneId = binReader.ReadInt64();
        if(Scene?.SceneID != sceneId)
            return;

        var id = binReader.ReadInt64();

        if(FindByID(id) is Entity entity)
        {
            Logger.JellyLogger.Warn("CONFLICT: received an EntityAdded packet for an entity that already exists, the local entity will be overwritten!");
        }
        else
        {
            entity = new();
            Add(entity);
        }

        entity.EntityID = id;
        entity.NetID = netId;
        entity.skipSync = true;

        entity.Enabled = binReader.ReadBoolean();
        entity.Visible = binReader.ReadBoolean();
        entity.Position = new(binReader.ReadInt32(), binReader.ReadInt32());
    }

    public void Clear()
    {
        Remove(Entities);
    }

    public bool Contains(Entity item)
    {
        return Entities.Contains(item);
    }

    void ICollection<Entity>.CopyTo(Entity[] array, int arrayIndex)
    {
        Entities.CopyTo(array, arrayIndex);
    }
}
