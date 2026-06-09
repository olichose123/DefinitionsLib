using System.Text.Json;
using System.Text.Json.Serialization;

namespace Olib.Godot.Definitions;

public class ReferenceJsonConverter : JsonConverterFactory
{
    private readonly IDefinitionProvider _definitionProvider;

    public ReferenceJsonConverter(IDefinitionProvider definitionProvider)
    {
        _definitionProvider = definitionProvider;
    }

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
        return (JsonConverter)Activator.CreateInstance(converterType, _definitionProvider)!;
    }

    private class ReferenceJsonConverterInner<T> : JsonConverter<Reference<T>> where T : Definition
    {
        private readonly IDefinitionProvider _definitionProvider;

        public ReferenceJsonConverterInner(IDefinitionProvider definitionProvider)
        {
            _definitionProvider = definitionProvider;
        }

        public override Reference<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? id = reader.GetString();
            if (id == null)
                return new Reference<T>(null, _definitionProvider);
            return new Reference<T>(id, _definitionProvider);
        }

        public override void Write(Utf8JsonWriter writer, Reference<T> value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Id);
        }
    }
}
