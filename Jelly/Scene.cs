using System;
using System.Text.Json.Serialization;

using Jelly.Coroutines;
using Jelly.Utilities;
using Microsoft.Xna.Framework;

namespace Jelly;

public class Scene
{
    private int width = 1;
    private int height = 1;

    /// <summary>
    /// Don't use this during multiplayer games
    /// </summary>
    [JsonIgnore] public bool Paused { get; set; }

    [JsonIgnore] public float TimeScale { get; set; } = 1;

    [JsonIgnore] public float TimeActive { get; private set; }

    [JsonIgnore] public float RawTimeActive { get; private set; }

    [JsonIgnore] public bool Focused { get; private set; }

    public int Width {
        get => width;
        set => width = MathHelper.Max(value, 1);
    }

    public int Height {
        get => height;
        set => height = MathHelper.Max(value, 1);
    }

    public CollisionWorld CollisionWorld { get; private set; }

    public EntityList Entities { get; }

    public string Name { get; set; } = "";

    [JsonIgnore] public CoroutineRunner CoroutineRunner { get; } = new();

    public event Action OnEndOfFrame;

    public Scene()
    {
        Entities = new(this);
        CollisionWorld = new(this);
    }

    public void Subscribe()
    {
        Focused = true;
    }

    public void Unsubscribe()
    {
        Focused = false;
    }

    public virtual void Begin()
    {
        Subscribe();

        foreach(var entity in Entities)
            entity.SceneBegin(this);
    }

    public virtual void End()
    {
        Unsubscribe();

        foreach (var entity in Entities)
            entity.SceneEnd(this);

        CoroutineRunner.StopAll();
    }

    public virtual void PreUpdate()
    {
        var delta = Time.DeltaTime;

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
        return (int)((TimeActive - (Time.DeltaTime * TimeScale)) / interval) < (int)(TimeActive / interval);
    }

    /// <summary>
    /// Returns whether the Scene timer has passed the given time interval since the last frame. Ex: given 2.0f, this will return true once every 2 seconds
    /// </summary>
    /// <param name="interval">The time interval to check for</param>
    /// <param name="offset">The time offset to start from</param>
    /// <returns></returns>
    public bool OnInterval(float interval, float offset)
    {
        return Math.Floor((TimeActive - offset - (Time.DeltaTime * TimeScale)) / interval) < Math.Floor((TimeActive - offset) / interval);
    }

    public bool OnRawInterval(float interval)
    {
        return (int)((RawTimeActive - Time.DeltaTime) / interval) < (int)(RawTimeActive / interval);
    }

    public bool OnRawInterval(float interval, float offset)
    {
        return Math.Floor((RawTimeActive - offset - Time.DeltaTime) / interval) < Math.Floor((RawTimeActive - offset) / interval);
    }

    #endregion
}
