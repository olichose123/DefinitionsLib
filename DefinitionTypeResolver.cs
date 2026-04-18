using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Definitions
{
    /// <summary>
    /// A Type Resolver for the Definition class. It allows for polymorphic serialization and deserialization of definitions, by using a type discriminator property called "$type". When a new definition type is encountered, it is added to the list of derived types for the Definition base type, allowing for correct serialization and deserialization of that type in the future. This allows for easy extension of the definition system with new types without needing to modify existing code.
    /// </summary>
    public class DefinitionTypeResolver : DefaultJsonTypeInfoResolver
    {
        static List<JsonDerivedType> DerivedTypes = new List<JsonDerivedType>();
        public static void AddDerivedType(Type derivedType, string discriminator)
        {
            DerivedTypes.Add(new JsonDerivedType(derivedType, discriminator));
        }

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
                foreach (var derivedType in DerivedTypes)
                {
                    jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(derivedType);
                }
            }

            return jsonTypeInfo;
        }
    }
}
