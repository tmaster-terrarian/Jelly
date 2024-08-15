using System.IO;
using System.Text.Json.Serialization;

using Microsoft.Xna.Framework;

using Jelly.Net;
using Jelly.Utilities;

namespace Jelly;

public class Entity : INetID
{
    public ComponentList Components { get; internal set; }

    private int depth;
    private Point position;

    internal bool skipSync;

    public Point Position
    {
        get => position;
        set {
            if(position != value)
            {
                position = value;
                MarkForSync();
            }
        }
    }

    public int NetID { get; internal set; }

    [JsonIgnore] public bool CanUpdateLocally => NetID == Providers.NetworkProvider.GetNetID();

    public long EntityID { get; internal set; } = Providers.IDRandom.NextInt64();

    public int Tag { get; set; }

    public bool Enabled { get; set; }
    public bool Visible { get; set; }

    [JsonIgnore] public Scene? Scene { get; private set; }

    public int Depth
    {
        get => depth;
        set
        {
            var val = MathHelper.Clamp(value, -100000, 100000);
            if(depth != val)
            {
                depth = val;
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

    [JsonIgnore] internal bool SyncThisStep { get; set; }
    [JsonIgnore] internal bool SyncImportant { get; set; }

    public Entity(Point position, int netID)
    {
        Position = position;
        NetID = netID < 0 ? Providers.NetworkProvider.GetHostNetID() : netID;
        Components = new(this);
    }

    public Entity(Point position) : this(position, -1) {}

    public Entity() : this(Point.Zero, -1) {}

    public void MarkForSync(bool important = false)
    {
        SyncThisStep = true;
        SyncImportant = important;
    }

    public virtual void Awake(Scene scene)
    {
        if(Components != null)
            foreach(var c in Components)
                c.EntityAwake();
    }

    public virtual void Added(Scene scene)
    {
        Scene = scene;
        if(Components != null)
            foreach(var c in Components)
                c.EntityAdded(scene);
    }

    public virtual void Removed(Scene scene)
    {
        if(Components != null)
            foreach(var c in Components)
                c.EntityRemoved(scene);
        Scene = null;
    }

    public virtual void SceneBegin(Scene scene) {}

    public virtual void SceneEnd(Scene scene)
    {
        if(Components != null)
            foreach(var c in Components)
                c.SceneEnd(scene);
    }

    public virtual void Update()
    {
        Components.Update();
    }

    public virtual void PreDraw()
    {
        Components.PreDraw();
    }

    public virtual void Draw()
    {
        Components.Draw();
    }

    public virtual void DrawUI()
    {
        Components.DrawUI();
    }

    public virtual void PostDraw()
    {
        Components.PostDraw();
    }

    public virtual bool TagIncludes(int tags)
    {
        return (tags & Tag) != 0;
    }

    public virtual bool TagMatches(int tags)
    {
        return (tags & Tag) == tags;
    }

    public void AddTag(int tag)
    {
        Tag |= tag;
    }

    public void RemoveTag(int tag)
    {
        Tag &= ~tag;
    }

    public void Remove(Component component)
    {
        Components.Remove(component);
    }

    public void Add(Component component)
    {
        Components.Add(component);
    }

    internal byte[] GetSyncPacket()
    {
        using var stream = new MemoryStream();
        var binWriter = new BinaryWriter(stream);

        binWriter.Write(Scene.SceneID);
        binWriter.Write(EntityID);

        binWriter.Write(Enabled);
        binWriter.Write(Visible);

        binWriter.Write(Position.X);
        binWriter.Write(Position.Y);

        return stream.ToArray();
    }
}
