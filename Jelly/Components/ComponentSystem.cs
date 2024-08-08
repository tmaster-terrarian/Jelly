using System.Collections.Generic;

namespace Jelly.Components;

public abstract class ComponentSystem<T> where T : Component
{
    private static readonly List<T> components = [];

    public static void Add(T component)
    {
        components.Add(component);
    }
}
