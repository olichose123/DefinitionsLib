using System;
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
        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

            Type basePointType = typeof(Definition);
            if (jsonTypeInfo.Type == basePointType)
            {
                jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
                {
                    TypeDiscriminatorPropertyName = "$type",
                    IgnoreUnrecognizedTypeDiscriminators = false,
                    UnknownDerivedTypeHandling = System.Text.Json.Serialization.JsonUnknownDerivedTypeHandling.FailSerialization,
                };

                var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(Definition)));

                foreach (var derivedType in types)
                {
                    jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(derivedType, derivedType.Name.ToLowerInvariant()));
                }
            }

            return jsonTypeInfo;
        }
    }
}
