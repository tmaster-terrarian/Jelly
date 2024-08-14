using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Jelly.Components;

namespace Jelly.Serialization;

public class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
{
    public HashSet<TypeSet> TypeSets { get; } = [];

    public TypeSet? GetTypeSet(Type type)
    {
        foreach(var typeSet in TypeSets)
            if(typeSet.BaseType == type || typeSet.DerivedTypes.Contains(type))
                return typeSet;
        return null;
    }

    public PolymorphicTypeResolver(IEnumerable<Type> types)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(types));

        foreach(var type in types)
        {
            if(type is null) throw new NullReferenceException($"{nameof(types)} cannot contain null values.");

            TypeSets.Add(new(type));
        }
    }

    public static HashSet<Type> GetDerivedTypesFromAssembly(Assembly assembly, Type baseType)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        HashSet<Type> list = [];

        foreach(var type in assembly.GetTypes())
        {
            if(type.IsClass && type.IsSubclassOf(baseType))
            {
                list.Add(type);
            }
        }

        return list;
    }

    public void GetAllDerivedTypesFromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        foreach(var typeSet in TypeSets)
        {
            foreach(var type in GetDerivedTypesFromAssembly(assembly, typeSet.BaseType))
            {
                if(type.IsClass)
                {
                    if(type.IsSubclassOf(typeSet.BaseType))
                    {
                        typeSet.DerivedTypes.Add(type);
                    }
                }
            }
        }
    }

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

        foreach(var typeSet in TypeSets)
        {
            if(jsonTypeInfo.Type == typeSet.BaseType)
            {
                typeSet.DerivedTypes ??= [];

                if(typeSet.DerivedTypes.Count == 0)
                {
                    return jsonTypeInfo;
                }

                var polyOptions = jsonTypeInfo.PolymorphismOptions ?? new JsonPolymorphismOptions
                {
                    IgnoreUnrecognizedTypeDiscriminators = false,
                    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                };

                foreach(var t in typeSet.DerivedTypes)
                {
                    if(t == typeSet.BaseType) continue;

                    if(!t.IsClass) continue;

                    if(!t.IsSubclassOf(typeSet.BaseType)) continue;

                    JsonDerivedType jsonDerivedType = new JsonDerivedType(t, t.Name);

                    // if(t.GetCustomAttribute<JsonAutoPolymorphicAttribute>(inherit: false) is JsonAutoPolymorphicAttribute autoPolymorphicAttribute)
                    // {
                    //     if(autoPolymorphicAttribute.TypeDiscriminator is string str)
                    //     {
                    //         jsonDerivedType = new JsonDerivedType(t, str);
                    //     }
                    //     else if(autoPolymorphicAttribute.TypeDiscriminator is int i)
                    //     {
                    //         jsonDerivedType = new JsonDerivedType(t, i);
                    //     }
                    // }

                    if(polyOptions.DerivedTypes.Contains(jsonDerivedType)) continue;

                    polyOptions.DerivedTypes.Add(jsonDerivedType);
                }

                jsonTypeInfo.PolymorphismOptions = polyOptions;

                return jsonTypeInfo;
            }
        }

        return jsonTypeInfo;
    }
}
