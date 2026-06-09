using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Olib.Godot.Definitions;

/// <summary>
/// Custom JSON type resolver for handling polymorphic serialization and deserialization of Definition objects. This resolver ensures that when a Definition object is serialized, its derived type information is included in the JSON output, allowing for correct deserialization back into the appropriate derived type. The DerivedTypes list must be populated with all the types that derive from Definition for this resolver to function correctly. If the DerivedTypes list is not initialized, an exception will be thrown to alert the developer to set it up before using the resolver.
/// </summary>
public class DefinitionTypeResolver : DefaultJsonTypeInfoResolver
{
    public static List<Type>? DerivedTypes;

    public static void RegisterType(Type derivedType)
    {
        if (DerivedTypes == null)
            DerivedTypes = new List<Type>();

        if (!DerivedTypes.Contains(derivedType))
            DerivedTypes.Add(derivedType);
    }

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
                    jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(derivedType, derivedType.Name));
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
