using System;
using System.Collections.Generic;
using System.Reflection;

namespace Jelly.Serialization;

public class TypeSet
{
    public HashSet<Type> DerivedTypes { get; set; } = [];

    public Type BaseType { get; }

    public TypeSet(Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        BaseType = type;

        if(BaseType.GetCustomAttribute<JsonAutoPolymorphicAttribute>(inherit: false) is null)
        {
            throw new Exception($"The specified type {BaseType} must have a {nameof(JsonAutoPolymorphicAttribute)} applied");
        }

        DerivedTypes = PolymorphicTypeResolver.GetDerivedTypesFromAssembly(Assembly.GetAssembly(BaseType), BaseType);
    }
}
