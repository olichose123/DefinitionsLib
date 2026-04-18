using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Definitions
{
    /// <summary>
    /// A Type Resolver for the Definition class. It overrides the GetTypeInfo method to add polymorphism options for the Definition class, allowing for derived types of Definition to be deserialized based on a type discriminator property in the JSON. The derived types are added to the polymorphism options based on the types that are found in the executing assembly that are subclasses of Definition. This allows for easy deserialization of JSON into the correct derived type of Definition based on the type discriminator property in the JSON.
    /// </summary>
    public class DefinitionTypeResolver : DefaultJsonTypeInfoResolver
    {
        public static List<Type>? DerivedTypes;

        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

            if (jsonTypeInfo.Type == typeof(Definition))
            {
                jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
                {
                    TypeDiscriminatorPropertyName = "$type",
                    IgnoreUnrecognizedTypeDiscriminators = false,
                    UnknownDerivedTypeHandling = System.Text.Json.Serialization.JsonUnknownDerivedTypeHandling.FailSerialization,
                };

                if (DerivedTypes != null)
                {
                    foreach (var derivedType in DerivedTypes)
                    {
                        jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(derivedType, derivedType.Name.ToLowerInvariant()));
                    }
                }
                else
                {
                    throw new Exception("DerivedTypes list is null. Please initialize it with the list of derived types of Definition before using the DefinitionTypeResolver.");
                }
            }

            return jsonTypeInfo;
        }
    }
}
