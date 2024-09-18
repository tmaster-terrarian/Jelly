// Modified from: https://github.com/JamesMcMahon/monocle-engine/blob/master/Monocle/InternalUtilities/ComponentList.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Jelly.Utilities;

public class EntityList : ICollection<Entity>, IEnumerable<Entity>, IEnumerable
{
    public static Comparison<Entity> CompareDepth => (a, b) => Math.Sign(b.Depth - a.Depth);

    private Dictionary<long, Entity> Entities { get; set; } = [];

    private readonly List<Entity> toAwake = [];

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

        if(exists || Entities.TryAdd(entity.EntityID, entity))
        {
            if(exists)
                Entities[entity.EntityID] = entity;

            if(Scene != null)
            {
                entity.Added(Scene);

                toAwake.Add(entity);
            }
        }
    }

    public bool Remove(Entity entity)
    {
        if(Entities.Remove(entity.EntityID))
        {
            if(Scene != null)
            {
                entity.Removed(Scene);
            }
            return true;
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

    public int AmountOf<T>() where T : Entity
    {
        int count = 0;
        foreach (var e in Entities.Values)
            if (e is T)
                count++;

        return count;
    }

    public int AmountOfType<T>() where T : Component
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

    public Entity FindFirstOfType<T>() where T : Component
    {
        foreach (var e in Entities.Values)
            foreach (var c in e.Components)
                if(c is T)
                    return e;

        return null;
    }

    public Entity FindByID(long id)
    {
        Entities.TryGetValue(id, out Entity entity);
        return entity;
    }

    public List<T> FindAll<T>() where T : Entity
    {
        List<T> list = [];

        foreach (var e in Entities.Values)
            if (e is T)
                list.Add(e as T);

        return list;
    }

    public List<Entity> FindAllOfType<T>() where T : Component
    {
        List<Entity> list = [];

        foreach (var e in Entities.Values)
            foreach (var c in e.Components)
                if(c is T)
                    list.Add(e);

        return list;
    }

    public void With<T>(Action<T> action) where T : Entity
    {
        foreach (var e in Entities.Values)
            if (e is T)
                action(e as T);
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
        foreach(var entity in Entities.Values)
            if(entity.Enabled)
                entity.Update();
    }

    private void Draw(int phase)
    {
        foreach(var entity in Entities.Values)
            if(entity.Visible)
                DrawPhase(entity, phase);
    }

    private void Draw(int phase, Tag matchTags, TagFilter filter)
    {
        foreach(var entity in Entities.Values)
            if(entity.Visible && entity.Tag.Matches(matchTags, filter))
                DrawPhase(entity, phase);
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

    internal void PreDraw(Tag matchTags, TagFilter filter) => Draw(0, matchTags, filter);

    internal void Draw() => Draw(1);

    internal void Draw(Tag matchTags, TagFilter filter) => Draw(1, matchTags, filter);

    internal void PostDraw() => Draw(2);

    internal void PostDraw(Tag matchTags, TagFilter filter) => Draw(2, matchTags, filter);

    internal void DrawUI() => Draw(3);

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
