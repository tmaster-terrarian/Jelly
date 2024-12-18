using System.IO;
using System.Text.Json.Serialization;

using Microsoft.Xna.Framework;

using Jelly.Net;
using Jelly.Utilities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Jelly;

public class Entity
{
    private int depth;

    public ComponentList Components { get; internal set; }

    public bool Persistent { get; set; }

    public Point Position { get; set; }

    public Point Pivot { get; set; } = Point.Zero;

    public Tag Tag { get; set; }

    public bool Enabled { get; set; } = true;
    public bool Visible { get; set; } = true;

    internal bool depthChanged = false;

    [JsonIgnore] public Scene? Scene { get; private set; }

    public int Depth
    {
        get => depth;
        set
        {
            var val = MathHelper.Clamp(value, -100000, 99999);
            if(depth != val)
            {
                depth = val;
                depthChanged = true;
            }
        }
    }

    [JsonIgnore]
    public int X
    {
        get => Position.X;
        set => Position = new(value, Position.Y);
    }

    [JsonIgnore]
    public int Y
    {
        get => Position.Y;
        set => Position = new(Position.X, value);
    }

    public long EntityID { get; set; } = JellyBackend.IDRandom.NextInt64();

    public Entity(Point position)
    {
        Position = position;
        Components = new(this);
        SetDefaults();
    }

    public Entity() : this(Point.Zero) {}

    public virtual void SetDefaults() {}

    public virtual void CopyTo(Entity other)
    {
        other.Position = Position;
        other.Depth = Depth;
        other.Enabled = Enabled;
        other.Scene = Scene;
        other.Visible = Visible;
        other.Tag = Tag;
    }

    public virtual void Awake(Scene scene)
    {
        bool wasLocked = Components.LockMode != ComponentList.LockModes.Open;
        if(!wasLocked)
            Components.LockMode = ComponentList.LockModes.Locked;

        foreach(var c in Components)
            c.EntityAwake();

        if(!wasLocked)
            Components.LockMode = ComponentList.LockModes.Open;
    }

    public virtual void Added(Scene scene)
    {
        Scene = scene;

        bool wasLocked = Components.LockMode != ComponentList.LockModes.Open;
        if(!wasLocked)
            Components.LockMode = ComponentList.LockModes.Locked;

        foreach(var c in Components)
            c.EntityAdded(scene);

        if(!wasLocked)
            Components.LockMode = ComponentList.LockModes.Open;
    }

    public virtual void Removed(Scene scene)
    {
        bool wasLocked = Components.LockMode != ComponentList.LockModes.Open;
        if(!wasLocked)
            Components.LockMode = ComponentList.LockModes.Locked;

        foreach(var c in Components)
            c.EntityRemoved(scene);

        if(!wasLocked)
            Components.LockMode = ComponentList.LockModes.Open;

        Scene = null;
    }

    public virtual void SceneBegin(Scene scene) {}

    public virtual void SceneEnd(Scene scene)
    {
        bool wasLocked = Components.LockMode != ComponentList.LockModes.Open;
        if(!wasLocked)
            Components.LockMode = ComponentList.LockModes.Locked;

        foreach(var c in Components)
            c.SceneEnd(scene);

        if(!wasLocked)
            Components.LockMode = ComponentList.LockModes.Open;
    }

    public virtual void Update()
    {
        Components?.Update();
    }

    public virtual void PreDraw()
    {
        Components?.PreDraw();
    }

    public virtual void Draw()
    {
        Components?.Draw();
    }

    public virtual void DrawUI()
    {
        Components?.DrawUI();
    }

    public virtual void PostDraw()
    {
        Components?.PostDraw();
    }

    public void RemoveComponent(Component component)
    {
        Components?.Remove(component);
    }

    public void AddComponent(Component component)
    {
        Components?.Add(component);
    }

    public T GetComponent<T>() where T : Component
    {
        return Components?.Get<T>();
    }

    public static IEqualityComparer<Entity> GetEqualityComparer() => new Comparer();

    class Comparer : IEqualityComparer<Entity>
    {
        public bool Equals(Entity x, Entity y)
        {
            return x.Equals(y);
        }

        public int GetHashCode([DisallowNull] Entity obj)
        {
            return obj.GetHashCode();
        }
    }
}
