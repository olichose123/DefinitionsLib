using System.Text.Json;
using System.Text.Json.Serialization;

namespace Definitions
{
    /// <summary>
    /// A Json converter for the Reference class. It serializes a Reference as the name of the definition it references, and deserializes a Reference from a string containing the name of the definition. This allows for easy serialization and deserialization of References in JSON, while still allowing for lazy loading of definitions when the Get method is called on the Reference.
    /// </summary>
    public class ReferenceJsonConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
                return false;

            if (typeToConvert.GetGenericTypeDefinition() != typeof(Reference<>))
                return false;

            return true;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            Type itemType = typeToConvert.GetGenericArguments()[0];
            Type converterType = typeof(ReferenceJsonConverterInner<>).MakeGenericType(itemType);
            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }

        private class ReferenceJsonConverterInner<T> : JsonConverter<Reference<T>> where T : Definition
        {
            public override Reference<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                string? name = reader.GetString();
                return new Reference<T>(name);
            }

            public override void Write(Utf8JsonWriter writer, Reference<T> value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.Name);
            }
        }
    }
}
