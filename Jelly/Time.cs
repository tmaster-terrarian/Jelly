using System;
using Microsoft.Xna.Framework;

namespace Jelly;

public static class Time
{
    private static float deltaTime;
    private static GameTime _gameTime;

    public static float TimeScale { get; set; } = 1f;

    public static float UnscaledDeltaTime => deltaTime;

    public static float DeltaTime => deltaTime * Math.Abs(TimeScale);

    public static GameTime GameTime => _gameTime;

    internal static void SetDeltaTime(GameTime gameTime)
    {
        deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _gameTime = gameTime;
    }
}
