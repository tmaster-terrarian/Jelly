namespace Jelly;

public static class Time
{
    private static float deltaTime;

    public static float TimeScale { get; set; }

    public static float UnscaledDeltaTime => deltaTime;

    public static float DeltaTime => deltaTime * TimeScale;

    internal static void SetDeltaTime(float delta)
    {
        deltaTime = delta;
    }
}
