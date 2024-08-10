// Modified from: https://github.com/JamesMcMahon/monocle-engine/blob/master/Monocle/InternalUtilities/ComponentList.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Jelly.IO;
using Jelly.Net;
using Microsoft.Xna.Framework;

namespace Jelly.Utilities;

public class EntityList : IEnumerable<Entity>, IEnumerable
{
    public static Comparison<Entity> CompareDepth => (a, b) => Math.Sign(b.Depth - a.Depth);

    private readonly List<Entity> entities = [];
    private readonly List<Entity> toAdd = [];
    private readonly List<Entity> toAwake = [];
    private readonly List<Entity> toRemove = [];

    private readonly HashSet<Entity> current = [];
    private readonly HashSet<Entity> adding = [];
    private readonly HashSet<Entity> removing = [];

    private bool unsorted;

    public Scene Scene { get; private set; }

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
                    entities.Add(entity);

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
                if(entities.Remove(entity))
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
            entities.Sort(CompareDepth);
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

    public void Add(Entity entity)
    {
        if (!adding.Contains(entity) && !current.Contains(entity))
        {
            adding.Add(entity);
            toAdd.Add(entity);
        }
    }

    public void Remove(Entity entity)
    {
        if (!removing.Contains(entity) && current.Contains(entity))
        {
            removing.Add(entity);
            toRemove.Add(entity);
        }
    }

    public void Add(IEnumerable<Entity> entities)
    {
        foreach (Entity entity in entities)
            Add(entity);
    }

    public void Remove(IEnumerable<Entity> entities)
    {
        foreach (Entity entity in entities)
            Remove(entity);
    }

    public void Add(params Entity[] entities)
    {
        foreach (Entity entity in entities)
            Add(entity);
    }

    public void Remove(params Entity[] entities)
    {
        foreach (Entity entity in entities)
            Remove(entity);
    }

    public int Count
    {
        get
        {
            return entities.Count;
        }
    }

    public Entity this[int index]
    {
        get
        {
            if (index < 0 || index >= entities.Count)
                throw new IndexOutOfRangeException();
            else
                return entities[index];
        }
    }

    public int AmountOf<T>() where T : Entity
    {
        int count = 0;
        foreach (var e in entities)
            if (e is T)
                count++;

        return count;
    }

    public T FindFirst<T>() where T : Entity
    {
        foreach (var e in entities)
            if (e is T)
                return e as T;

        return null;
    }

    public Entity FindByID(long id)
    {
        foreach (var e in entities)
            if (e.EntityID == id)
                return e;

        return null;
    }

    public List<T> FindAll<T>() where T : Entity
    {
        List<T> list = [];

        foreach (var e in entities)
            if (e is T)
                list.Add(e as T);

        return list;
    }

    public void With<T>(Action<T> action) where T : Entity
    {
        foreach (var e in entities)
            if (e is T)
                action(e as T);
    }

    public IEnumerator<Entity> GetEnumerator()
    {
        return entities.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Entity[] ToArray()
    {
        return [.. entities];
    }

    public bool HasVisibleEntities(int matchTags, TagFilter filter = TagFilter.AtLeastOne)
    {
        switch(filter)
        {
            case TagFilter.AtLeastOne:
                foreach(var entity in entities)
                    if(entity.Visible && entity.TagIncludes(matchTags))
                        return true;
                break;

            case TagFilter.All:
                foreach(var entity in entities)
                    if(entity.Visible && entity.TagMatches(matchTags))
                        return true;
                break;

            case TagFilter.None:
                foreach(var entity in entities)
                    if(entity.Visible && !entity.TagIncludes(matchTags))
                        return true;
                break;
        }

        return false;
    }

    internal void Update()
    {
        foreach(var entity in entities)
            if(entity.Enabled && entity.CanUpdateLocally)
                entity.Update();
    }

    private void Draw(int phase)
    {
        foreach(var entity in entities)
            if(entity.Visible)
                DrawPhase(entity, phase);
    }

    private void Draw(int phase, int matchTags, TagFilter filter)
    {
        switch(filter)
        {
            case TagFilter.AtLeastOne:
                foreach(var entity in entities)
                    if(entity.Visible && entity.TagIncludes(matchTags))
                        DrawPhase(entity, phase);
                break;

            case TagFilter.All:
                foreach(var entity in entities)
                    if(entity.Visible && entity.TagMatches(matchTags))
                        DrawPhase(entity, phase);
                break;

            case TagFilter.None:
                foreach(var entity in entities)
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
        for(int i = 0; i < entities.Count; i++)
        {
            Entity entity = entities[i];
            if (entity.Enabled && entity.SyncThisStep)
            {
                Providers.NetworkProvider.SendSyncPacket(SyncPacketType.EntityUpdate, entity.GetSyncPacket(), entity.SyncImportant);

                entity.SyncThisStep = false;
                entity.SyncImportant = false;
            }
        }
    }

    internal void ReadPacket(byte[] data, int sender, bool newEntity = false)
    {
        using var binReader = new BinaryReader(new MemoryStream(data));

        var id = binReader.ReadInt64();
        var entity = FindByID(id);
        if(entity is null)
        {
            if(!newEntity)
            {
                Logger.JellyLogger.Error("DESYNC: received a packet for an entity that doesn't exist!");
                return;
            }

            entity = new(Point.Zero, sender) {
                EntityID = id,
                skipSync = true
            };

            Add(entity);
        }

        entity.Position = new(binReader.ReadInt32(), binReader.ReadInt32());

        // Logger.JellyLogger.Warn("An entity was given an invalid packet and was not updated. This may cause a desync!");
    }
}
