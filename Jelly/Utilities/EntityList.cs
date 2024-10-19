// Modified from: https://github.com/JamesMcMahon/monocle-engine/blob/master/Monocle/InternalUtilities/EntityList.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Jelly.Utilities;

public class EntityList : ICollection<Entity>, IEnumerable<Entity>, IEnumerable
{
    [JsonConverter(typeof(JsonStringEnumConverter<LockModes>))]
    public enum LockModes { Open, Locked, Error };

    private static Comparison<Entity> CompareDepth => (a, b) => Math.Sign(b.Depth - a.Depth);

    [JsonIgnore]
    internal List<Entity> ToDraw
    {
        get
        {
            if(!_drawOrderDirty) return toDraw;

            toDraw = [..Entities.Values];
            toDraw.Sort(CompareDepth);

            _drawOrderDirty = false;

            return toDraw;
        }
    }

    private bool _drawOrderDirty = true;

    private List<Entity> toDraw;

    private Dictionary<long, Entity> entities = [];

    private Dictionary<long, Entity> Entities
    {
        get => entities;
        set
        {
            entities = value;
            _drawOrderDirty = true;
        }
    }

    private readonly List<Entity> toAwake = [];
    private readonly List<Entity> toAdd = [];
    private readonly List<Entity> toRemove = [];

    private readonly HashSet<Entity> current = new HashSet<Entity>(Entity.GetEqualityComparer());
    private readonly HashSet<Entity> adding = new HashSet<Entity>(Entity.GetEqualityComparer());
    private readonly HashSet<Entity> removing = new HashSet<Entity>(Entity.GetEqualityComparer());

    private LockModes lockMode;

    public LockModes LockMode
    {
        get => lockMode;

        internal set
        {
            lockMode = value;

            if (toAdd.Count > 0)
            {
                foreach (var entity in toAdd)
                {
                    if (current.Add(entity))
                    {
                        entities.Add(entity.EntityID, entity);
                        entity.Added(Scene);
                    }
                }

                adding.Clear();
                toAdd.Clear();

                _drawOrderDirty = true;
            }

            if (toRemove.Count > 0)
            {
                foreach (var entity in toRemove)
                {
                    if (current.Remove(entity))
                    {
                        entities.Remove(entity.EntityID);
                        entity.Removed(Scene);
                    }
                }

                removing.Clear();
                toRemove.Clear();

                _drawOrderDirty = true;
            }
        }
    }

    [JsonIgnore] public Scene Scene { get; internal set; }

    [JsonIgnore] public int Count => Entities.Count;

    public bool IsReadOnly { get; }

    [JsonIgnore]
    public Entity this[int index]
    {
        get
        {
            if (index < 0 || index >= Entities.Count)
                throw new IndexOutOfRangeException();
            else
                return ((Entity[])[..Entities.Values])[index];
        }
    }

    internal EntityList(Scene scene)
    {
        Scene = scene;
    }

    public void UpdateLists()
    {
        if(toAwake.Count > 0)
        {
            foreach(var entity in toAwake)
                if(entity.Scene == Scene)
                    entity.Awake(Scene);

            toAwake.Clear();
        }
    }

    public int IndexOf(Entity entity)
    {
        int i = -1;
        foreach(var e in Entities.Values)
        {
            i++;

            if(e is null) continue;

            if(e == entity)
                return i;
        }
        return -1;
    }

    public void Add(Entity entity)
    {
        bool exists = Entities.ContainsKey(entity.EntityID);

        switch (LockMode)
        {
            case LockModes.Open:
                if(exists || Entities.TryAdd(entity.EntityID, entity))
                {
                    _drawOrderDirty = true;

                    if(exists)
                        Entities[entity.EntityID] = entity;

                    if(Scene != null)
                    {
                        entity.Added(Scene);

                        toAwake.Add(entity);
                    }
                }
                break;

            case LockModes.Locked:
                if (!current.Contains(entity) && !adding.Contains(entity))
                {
                    adding.Add(entity);
                    toAdd.Add(entity);
                    toAwake.Add(entity);
                }
                break;

            case LockModes.Error:
                throw new Exception("Cannot add or remove Entities at this time!");
        }
    }

    public bool Remove(Entity entity)
    {
        switch (LockMode)
        {
            case LockModes.Open:
                if(Entities.Remove(entity.EntityID))
                {
                    _drawOrderDirty = true;

                    if(Scene != null)
                    {
                        entity.Removed(Scene);
                    }

                    return true;
                }
                break;

            case LockModes.Locked:
                if (current.Contains(entity) && !removing.Contains(entity))
                {
                    removing.Add(entity);
                    toRemove.Add(entity);

                    return true;
                }
                break;

            case LockModes.Error:
                throw new Exception("Cannot add or remove Entities at this time!");
        }

        return false;
    }

    public void AddRange(IEnumerable<Entity> entities)
    {
        foreach (Entity entity in entities)
            Add(entity);
    }

    public bool RemoveRange(IEnumerable<Entity> entities)
    {
        bool result = false;
        foreach (Entity entity in entities)
            result |= Remove(entity);
        return result;
    }

    public void AddRange(params Entity[] entities)
    {
        foreach (Entity entity in entities)
            Add(entity);
    }

    public bool RemoveRange(params Entity[] entities)
    {
        bool result = false;
        foreach (Entity entity in entities)
            result |= Remove(entity);
        return result;
    }

    public Entity FindByID(long id)
    {
        return Entities.TryGetValue(id, out Entity entity) ? entity : null;
    }

    public int AmountOf<T>() where T : Entity
    {
        int count = 0;
        foreach (var e in Entities.Values)
            if (e is T)
                count++;

        return count;
    }

    public int AmountOfWithComponent<T>() where T : Component
    {
        int count = 0;
        foreach (var e in Entities.Values)
            foreach (var c in e.Components)
                if(c is T)
                    count++;

        return count;
    }

    public T FindFirst<T>() where T : Entity
    {
        foreach (var e in Entities.Values)
            if (e is T)
                return e as T;

        return null;
    }

    public Entity FindFirstWithComponent<T>() where T : Component
    {
        foreach (var e in Entities.Values)
            foreach (var c in e.Components)
                if(c is T)
                    return e;

        return null;
    }

    public List<T> FindAll<T>() where T : Entity
    {
        List<T> list = [];

        foreach (var e in Entities.Values)
            if (e is T)
                list.Add(e as T);

        return list;
    }

    public List<Entity> FindAllWithComponent<T>() where T : Component
    {
        List<Entity> list = [];

        foreach (var e in Entities.Values)
            foreach (var c in e.Components)
                if(c is T)
                    list.Add(e);

        return list;
    }

    public void Foreach<T>(Action<T> action) where T : Entity
    {
        bool wasLocked = LockMode != LockModes.Open;
        if(!wasLocked)
            LockMode = LockModes.Locked;

        foreach (var e in Entities.Values)
            if (e is T)
                action?.Invoke(e as T);

        if(!wasLocked)
            LockMode = LockModes.Open;
    }

    public void ForeachWithComponent<T>(Action<Entity> action) where T : Component
    {
        bool wasLocked = LockMode != LockModes.Open;
        if(!wasLocked)
            LockMode = LockModes.Locked;

        foreach (var e in FindAllWithComponent<T>())
            action?.Invoke(e);

        if(!wasLocked)
            LockMode = LockModes.Open;
    }

    public IEnumerator<Entity> GetEnumerator()
    {
        return Entities.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Entity[] ToArray()
    {
        return [.. Entities.Values];
    }

    public bool HasVisibleEntities(Tag matchTags, TagFilter filter = TagFilter.AtLeastOne)
    {
        foreach(var entity in Entities.Values)
            if(entity.Visible && entity.Tag.Matches(matchTags, filter))
                return true;

        return false;
    }

    internal void Update()
    {
        LockMode = LockModes.Locked;
        foreach(var entity in Entities.Values)
            if(entity.Enabled)
                entity.Update();
        LockMode = LockModes.Open;

        foreach(var entity in Entities.Values)
        {
            if(entity.depthChanged)
            {
                entity.depthChanged = false;
                _drawOrderDirty = true;
            }
        }
    }

    private void Draw(int phase, Tag matchTags, TagFilter filter)
    {
        LockMode = LockModes.Error;
        foreach(var entity in ToDraw)
            if(entity.Visible && entity.Tag.Matches(matchTags, filter))
                DrawPhase(entity, phase);
        LockMode = LockModes.Open;
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

    internal void PreDraw() => Draw(0, Tag.Empty, TagFilter.NoFiltering);

    internal void PreDraw(Tag matchTags, TagFilter filter) => Draw(0, matchTags, filter);

    internal void Draw() => Draw(1, Tag.Empty, TagFilter.NoFiltering);

    internal void Draw(Tag matchTags, TagFilter filter) => Draw(1, matchTags, filter);

    internal void PostDraw() => Draw(2, Tag.Empty, TagFilter.NoFiltering);

    internal void PostDraw(Tag matchTags, TagFilter filter) => Draw(2, matchTags, filter);

    internal void DrawUI() => Draw(3, Tag.Empty, TagFilter.NoFiltering);

    internal void DrawUI(Tag matchTags, TagFilter filter) => Draw(3, matchTags, filter);

    public void Clear()
    {
        RemoveRange(Entities.Values);
    }

    public bool Contains(Entity item)
    {
        return Entities.ContainsValue(item);
    }

    void ICollection<Entity>.CopyTo(Entity[] array, int arrayIndex)
    {
        Entities.Values.CopyTo(array, arrayIndex);
    }
}
