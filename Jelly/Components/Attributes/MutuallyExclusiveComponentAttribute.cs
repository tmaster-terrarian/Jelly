using System;

namespace Jelly.Components.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class MutuallyExclusiveComponentAttribute(Type type) : ComponentTypeAttribute(type)
{
    public MutuallyExclusiveComponentKind ExclusionKind { get; set; } = MutuallyExclusiveComponentKind.Default;
}

public enum MutuallyExclusiveComponentKind
{
    /// <summary>
    /// Automatically remove conflicting <see cref="Component"/>s when adding an instance of this <see cref="Component"/> class.
    /// </summary>
    Default,

    /// <summary>
    /// Don't resolve conflicting <see cref="Component"/>s and log a warning.
    /// </summary>
    Warn,

    /// <summary>
    /// Don't resolve conflicting <see cref="Component"/>s and throw an Exception.
    /// </summary>
    Throw,
}
