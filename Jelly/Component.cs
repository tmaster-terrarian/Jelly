using System.IO;
using System.Text.Json.Serialization;

using Jelly.Net;
using Jelly.Serialization;

namespace Jelly;

[JsonAutoPolymorphic]
public abstract class Component : INetID
{
    internal Entity entity;

    internal bool enabled;
    internal bool skipSync;

    [JsonInclude] public long ComponentID { get; internal set; } = Providers.IDRandom.NextInt64();

    [JsonIgnore] public bool SyncThisStep { get; internal set; }
    [JsonIgnore] public bool SyncImportant { get; internal set; }

    [JsonIgnore] public bool CanUpdateLocally => Entity.CanUpdateLocally;

    [JsonIgnore] public int NetID => Entity.NetID;

    [JsonIgnore]
    public Entity Entity
    {
        get => entity;

        private set
        {
            if(ReferenceEquals(value, entity) || value == entity)
            {
                return;
            }

            entity = value;
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

    [JsonIgnore] public Scene? Scene => Entity?.Scene;

    /// <summary>
    /// Ensure that the zero-argument constructor is preserved in all subtypes, to avoid netcode problems.
    /// </summary>
    public Component() : this(true, true) {}

    public Component(bool enabled, bool visible)
    {
        Enabled = enabled;
        Visible = visible;
    }

    public void MarkForSync(bool important = false)
    {
        SyncThisStep = true;
        SyncImportant = important;
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

    /// <summary>
    /// Called every frame on every client. Use <see cref="CanUpdateLocally"/> to prevent problems
    /// </summary>
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
        Entity?.Components?.Remove(this);
    }

    internal byte[] GetInternalSyncPacket()
    {
        using var stream = new MemoryStream();
        var binWriter = new BinaryWriter(stream);

        binWriter.Write(Scene.SceneID);
        binWriter.Write(Entity.EntityID);
        binWriter.Write(ComponentID);

        var name = GetType().FullName;

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
