using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

using Microsoft.Xna.Framework;

using Jelly.Coroutines;
using Jelly.Net;
using Jelly.Utilities;

namespace Jelly;

public class Scene
{
    /// <summary>
    /// Don't use this during multiplayer games
    /// </summary>
    [JsonIgnore] public bool Paused { get; set; }

    [JsonIgnore] public float TimeScale { get; set; } = 1;

    [JsonIgnore] public float TimeActive { get; private set; }

    [JsonIgnore] public float RawTimeActive { get; private set; }

    [JsonIgnore] public bool Focused { get; private set; }

    public EntityList Entities { get; }

    public string Name { get; set; } = "";

    [JsonIgnore] public CoroutineRunner CoroutineRunner { get; } = new();

    [JsonIgnore] public long SceneID { get; }

    public event Action OnEndOfFrame;

    public Scene()
    {
        Entities = new(this);
        SceneID = Name.GetHashCode();
    }

    public Scene(long idOverride) : this()
    {
        SceneID = idOverride;
    }

    public void Subscribe()
    {
        Focused = true;
        Providers.NetworkProvider.PacketReceived += ReadPacket;
    }

    public void Unsubscribe()
    {
        Focused = false;
        Providers.NetworkProvider.PacketReceived -= ReadPacket;
    }

    public virtual void Begin()
    {
        Subscribe();

        foreach (var entity in Entities)
            if(entity.CanUpdateLocally)
                entity.SceneBegin(this);
    }

    public virtual void End()
    {
        Unsubscribe();

        foreach (var entity in Entities)
            if(entity.CanUpdateLocally)
                entity.SceneEnd(this);

        CoroutineRunner.StopAll();
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

        if(!Paused && Providers.NetworkProvider.NetworkingEnabled)
        {
            SendPackets();
        }
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

    private void SendPackets()
    {
        foreach(var entity in Entities)
        {
            if(entity.CanUpdateLocally && entity.Enabled && entity.SyncThisStep)
            {
                Providers.NetworkProvider.SendSyncPacket(SyncPacketType.EntityUpdate, entity.GetInternalSyncPacket(), entity.SyncImportant);

                entity.SyncThisStep = false;
                entity.SyncImportant = false;
            }
        }
    }

    private void ReadPacket(byte[] data, int sender)
    {
        if(!Providers.NetworkProvider.NetworkingEnabled)
            return;

        SyncPacketType type = (SyncPacketType)data[0];
        var payload = data[1..];
    }
}
