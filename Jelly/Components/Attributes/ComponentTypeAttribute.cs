using System;

namespace Jelly.Components.Attributes;

public class ComponentTypeAttribute(Type type) : Attribute
{
    public Type ComponentType { get; set; } = (type is not null && type.IsClass && type.IsSubclassOf(typeof(Component))) ? type : null;
}
