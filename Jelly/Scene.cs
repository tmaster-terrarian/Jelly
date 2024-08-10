using System;
using System.Collections.Generic;
using System.IO;
using Jelly.Coroutines;
using Jelly.Net;
using Jelly.Utilities;
using Microsoft.Xna.Framework;

namespace Jelly;

public class Scene
{
    /// <summary>
    /// Don't use this during multiplayer games
    /// </summary>
    public bool Paused { get; set; }

    public float TimeScale { get; set; } = 1;

    public float TimeActive { get; private set; }

    public float RawTimeActive { get; private set; }

    public bool Focused { get; private set; }

    public EntityList Entities { get; private set; }

    public Entity HelperEntity { get; private set; }

    public CoroutineRunner CoroutineRunner { get; } = new();

    public event Action OnEndOfFrame;

    public Scene()
    {
        Entities = new(this);
    }

    public virtual void Begin()
    {
        Focused = true;
        Providers.PacketReceived += ReadPacket;

        foreach (var entity in Entities)
            entity.SceneBegin(this);
    }

    public virtual void End()
    {
        Focused = false;
        Providers.PacketReceived -= ReadPacket;

        foreach (var entity in Entities)
            entity.SceneEnd(this);
    }

    public virtual void PreUpdate()
    {
        var delta = Providers.DeltaTime;

        RawTimeActive += delta;
        if(!Paused)
        {
            TimeActive += delta;
            CoroutineRunner.Update(delta);
        }

        Entities.UpdateLists();
        // TagLists.UpdateLists();
    }

    public virtual void Update()
    {
        if(!Paused)
        {
            Entities.Update();
        }
    }

    public virtual void PostUpdate()
    {
        OnEndOfFrame?.Invoke();
        OnEndOfFrame = null;

        Entities.SendPackets();
    }

    public virtual void PreDraw()
    {
        Entities.PreDraw();
    }

    public virtual void Draw()
    {
        Entities.Draw();
    }

    public virtual void PostDraw()
    {
        Entities.PostDraw();
    }

    public virtual void DrawUI()
    {
        Entities.DrawUI();
    }

    public virtual void GainFocus() {}

    public virtual void LoseFocus() {}

    #region Interval

    /// <summary>
    /// Returns whether the Scene timer has passed the given time interval since the last frame. Ex: given 2.0f, this will return true once every 2 seconds
    /// </summary>
    /// <param name="interval">The time interval to check for</param>
    /// <returns></returns>
    public bool OnInterval(float interval)
    {
        return (int)((TimeActive - (Providers.DeltaTime * TimeScale)) / interval) < (int)(TimeActive / interval);
    }

    /// <summary>
    /// Returns whether the Scene timer has passed the given time interval since the last frame. Ex: given 2.0f, this will return true once every 2 seconds
    /// </summary>
    /// <param name="interval">The time interval to check for</param>
    /// <param name="offset">The time offset to start from</param>
    /// <returns></returns>
    public bool OnInterval(float interval, float offset)
    {
        return Math.Floor((TimeActive - offset - (Providers.DeltaTime * TimeScale)) / interval) < Math.Floor((TimeActive - offset) / interval);
    }

    public bool OnRawInterval(float interval)
    {
        return (int)((RawTimeActive - Providers.DeltaTime) / interval) < (int)(RawTimeActive / interval);
    }

    public bool OnRawInterval(float interval, float offset)
    {
        return Math.Floor((RawTimeActive - offset - Providers.DeltaTime) / interval) < Math.Floor((RawTimeActive - offset) / interval);
    }

    #endregion

    private void ReadPacket(byte[] data, int sender)
    {
        SyncPacketType type = (SyncPacketType)data[0];
        var payload = data[1..];

        switch(type)
        {
            case SyncPacketType.EntityUpdate:
            {
                Entities.ReadPacket(payload, sender);
                break;
            }
            case SyncPacketType.EntityAdded:
            {
                Entities.ReadPacket(payload, sender, true);
                break;
            }
            case SyncPacketType.EntityRemoved:
            {
                using var stream = new MemoryStream(payload);

                long entity = new BinaryReader(stream).ReadInt64();
                Entities.Remove(Entities.FindByID(entity));
                break;
            }
            case SyncPacketType.ComponentUpdate:
            {
                using var stream = new MemoryStream(payload);

                long entity = new BinaryReader(stream).ReadInt64();
                Entities.FindByID(entity)?.Components?.ReadPacket(payload[8..]);
                break;
            }
            case SyncPacketType.ComponentAdded:
            {
                using var stream = new MemoryStream(payload);

                long entity = new BinaryReader(stream).ReadInt64();
                Entities.FindByID(entity)?.Components?.ReadPacket(payload[8..], true);
                break;
            }
            case SyncPacketType.ComponentRemoved:
            {
                using var stream = new MemoryStream(payload);

                long entity = new BinaryReader(stream).ReadInt64();
                Entities.FindByID(entity)?.Components?.ReadRemovalPacket(payload[8..]);
                break;
            }
        }
    }
}
