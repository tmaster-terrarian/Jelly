using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Jelly.Components;

namespace Jelly.Serialization;

public class PolymorphicTypeResolver(Type baseType) : DefaultJsonTypeInfoResolver
{
    public IList<Type> DerivedTypes { get; set; } = [];

    public Type BaseType { get; } = baseType;

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

        if(jsonTypeInfo.Type == BaseType)
        {
            var polyOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "$type",
                IgnoreUnrecognizedTypeDiscriminators = false,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                DerivedTypes = {
                    new JsonDerivedType(typeof(SpriteComponent), nameof(SpriteComponent))
                }
            };

            foreach(var t in DerivedTypes)
            {
                polyOptions.DerivedTypes.Add(new JsonDerivedType(t, t.Name));
            }

            jsonTypeInfo.PolymorphismOptions = polyOptions;
        }

        return jsonTypeInfo;
    }
}
