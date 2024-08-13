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
    public HashSet<Type> DerivedTypes { get; set; } = [];

    public Type BaseType { get; }

    public PolymorphicTypeResolver(Type type)
    {
        BaseType = type;

        if(BaseType.GetCustomAttribute<JsonAutoPolymorphicAttribute>(inherit: false) is null)
        {
            throw new Exception($"The specified type {BaseType} must have a {nameof(JsonAutoPolymorphicAttribute)} applied");
        }

        DerivedTypes = GetDerivedTypesFromAssembly(Assembly.GetAssembly(BaseType));
    }

    public HashSet<Type> GetDerivedTypesFromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        HashSet<Type> list = [];

        foreach(var type in assembly.GetTypes())
        {
            if(type.IsClass && type.IsSubclassOf(BaseType))
            {
                list.Add(type);
            }
        }

        return list;
    }

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

        if(jsonTypeInfo.Type == BaseType)
        {
            DerivedTypes ??= [];

            if(DerivedTypes.Count == 0)
            {
                return jsonTypeInfo;
            }

            var polyOptions = jsonTypeInfo.PolymorphismOptions ?? new JsonPolymorphismOptions
            {
                IgnoreUnrecognizedTypeDiscriminators = false,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
            };

            foreach(var t in DerivedTypes)
            {
                if(t is null)
                {
                    throw new NullReferenceException();
                }

                if(t == BaseType) continue;

                if(!t.IsClass) continue;

                if(!t.IsSubclassOf(BaseType)) continue;

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
        }

        return jsonTypeInfo;
    }
}
