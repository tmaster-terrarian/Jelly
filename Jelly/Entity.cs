using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;

using Jelly.Components;
using Jelly.IO;
using Jelly.Net;
using Jelly.Utilities;

namespace Jelly;

public class Entity : INetworkedObject
{
    public ComponentList Components { get; }

    private bool markedForRemoval;
    private bool syncThisStep;
    private bool syncImportant;
    private int depth;

    public Point Position { get; set; }

    /// <summary>
    /// Corresponds to the index of the player that owns this entity.
    /// </summary>
    public int NetID { get; }

    public bool Enabled { get; private set; }

    public int Depth
    {
        get => depth;
        set
        {
            if(depth != value)
            {
                depth = value;
                // Scene?.SetActualDepth(this);
            }
        }
    }

    public float RendererDepth { get; set; }

    public int X
    {
        get => Position.X;
        set => Position = new(value, Position.Y);
    }

    public int Y
    {
        get => Position.Y;
        set => Position = new(Position.X, value);
    }

    public Entity(Point position, int netID)
    {
        Position = position;
        NetID = netID < 0 ? Providers.NetworkProvider.GetHostNetID() : netID;
        Components = new(this);
    }

    public Entity(Point position) : this(position, -1) {}

    public Entity() : this(Point.Zero, -1) {}

    // public static void UpdateAll(GameTime gameTime)
    // {
    //     for(int i = 0; i < entities.Length; i++)
    //     {
    //         var entity = entities[i];

    //         if(entity.NetID != Providers.NetworkProvider.GetNetID()) continue;

    //         if(!entity.Enabled) continue;

    //         // ...

    //         if(entity.syncThisStep)
    //         {
    //             Providers.NetworkProvider.SendSyncPacket(entity.GetSyncPacket(), entity.syncImportant);

    //             entity.syncThisStep = false;
    //             entity.syncImportant = false;
    //         }
    //     }
    // }

    public virtual void Update(GameTime gameTime)
    {
        Components.Update(gameTime);
    }

    public virtual void Draw(GameTime gameTime)
    {
        Components.Draw(gameTime);
    }

    public byte[] GetSyncPacket()
    {
        var binWriter = new BinaryWriter(new BinaryStream());

        binWriter.Write(Position.X);
        binWriter.Write(Position.Y);

        byte[] value = ((BinaryStream)binWriter.BaseStream).Buffer;

        binWriter.Dispose();

        return value;
    }

    public void ReadSyncPacket(byte[] data)
    {
        var binReader = new BinaryReader(new BinaryStream(data));

        Position = new(binReader.ReadInt32(), binReader.ReadInt32());

        binReader.Dispose();
    }

    public void RemoveComponent(Component component)
    {
        if(ReferenceEquals(component.Entity, this) || component.Entity == this)
        {
            component.Entity = null;
            component.Enabled = false;
        }

        Components.Remove(component);
    }

    public void AddComponent(Component component)
    {
        Components.Add(component);
        component.Entity = this;
    }
}
