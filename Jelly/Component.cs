using System.IO;

using Jelly.Net;

namespace Jelly;

public abstract class Component : INetID
{
    internal Entity entity;

    internal bool enabled;

    internal long ComponentID { get; set; }

    public bool SyncThisStep { get; internal set; }
    public bool SyncImportant { get; internal set; }

    internal bool skipSync;

    public int NetID => Entity.NetID;

    public Entity Entity
    {
        get => entity;

        private set
        {
            if(value is not null)
            {
                if(ReferenceEquals(value, entity) || value == entity)
                {
                    return;
                }
            }
            else if(entity is null)
            {
                return;
            }

            if(value is not null)
            {
                entity = value;
                Added(value);
            }
            else
            {
                Removed(entity);
                entity = value;
            }
        }
    }

    public bool Enabled
    {
        get => enabled;
        set {
            if(enabled == value)
                return;

            enabled = value;

            if(enabled)
                OnEnable();
            else
                OnDisable();
        }
    }

    public bool Visible { get; set; } = true;

    public Scene Scene => Entity?.Scene;

    /// <summary>
    /// Ensure that the zero-argument constructor is preserved in all subtypes, to avoid netcode problems.
    /// </summary>
    public Component()
    {
        Enabled = true;
        Visible = true;
    }

    public Component(bool enabled, bool visible)
    {
        Enabled = enabled;
        Visible = visible;
    }

    public virtual void Added(Entity entity)
    {
        Entity = entity;
    }

    public virtual void Removed(Entity entity)
    {
        Entity = null;
    }

    public virtual void EntityAwake() {}

    public virtual void EntityAdded(Scene scene) {}

    public virtual void EntityRemoved(Scene scene) {}

    public virtual void OnEnable() {}

    public virtual void OnDisable() {}

    public virtual void Update() {}

    /// <summary>
    /// Will not be called if the running instance is headless or if the component is not visible
    /// </summary>
    public virtual void PreDraw() {}

    /// <summary>
    /// Will not be called if the running instance is headless or if the component is not visible
    /// </summary>
    public virtual void Draw() {}

    /// <summary>
    /// Will not be called if the running instance is headless or if the component is not visible
    /// </summary>
    public virtual void PostDraw() {}

    /// <summary>
    /// Will not be called if the running instance is headless or if the component is not visible
    /// </summary>
    public virtual void DrawUI() {}

    public virtual void SceneEnd(Scene scene) {}

    public void RemoveSelf()
    {
        Entity?.Remove(this);
    }

    internal byte[] GetInternalSyncPacket()
    {
        using var stream = new MemoryStream();
        var binWriter = new BinaryWriter(stream);

        binWriter.Write(Entity.EntityID);
        binWriter.Write(ComponentID);

        var name = GetType().FullName;

        // binWriter.Write((byte)Math.Clamp(name.Length, 0, 255));
        binWriter.Write(name);

        binWriter.Write(Enabled);
        binWriter.Write(Visible);

        return [
            ..stream.ToArray(),
            ..(GetSyncPacket() ?? [])
        ];
    }

    public virtual byte[] GetSyncPacket() => [];

    public virtual void ReadPacket(byte[] data) {}
}
