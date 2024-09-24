using System;

namespace Jelly.Components.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class RequiredComponentAttribute(Type type) : ComponentTypeAttribute(type)
{
    
}
