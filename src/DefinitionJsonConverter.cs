using System.Text.Json;
using System.Text.Json.Serialization;

namespace Olib.Godot.Definitions;

public class DefinitionJsonConverterFactory : JsonConverterFactory
{
    private readonly IDefinitionProvider _definitionProvider;

    public DefinitionJsonConverterFactory(IDefinitionProvider definitionProvider)
    {
        _definitionProvider = definitionProvider;
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(Definition).IsAssignableFrom(typeToConvert);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type converterType = typeof(DefinitionJsonConverterInner<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType, _definitionProvider)!;
    }

    private class DefinitionJsonConverterInner<T> : JsonConverter<T> where T : Definition
    {
        private readonly IDefinitionProvider _definitionProvider;

        public DefinitionJsonConverterInner(IDefinitionProvider definitionProvider)
        {
            _definitionProvider = definitionProvider;
        }

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var modifiedOptions = new JsonSerializerOptions(options);
            foreach (var converter in modifiedOptions.Converters)
            {
                if (converter is DefinitionJsonConverterFactory)
                {
                    modifiedOptions.Converters.Remove(converter);
                    break;
                }
            }

            T? definition = JsonSerializer.Deserialize(ref reader, typeof(Definition), modifiedOptions) as T ?? throw new JsonException($"Failed to deserialize {typeToConvert.Name}");
            _definitionProvider.Register(definition);
            return definition;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var modifiedOptions = new JsonSerializerOptions(options);
            foreach (var converter in modifiedOptions.Converters)
            {
                if (converter is DefinitionJsonConverterFactory)
                {
                    modifiedOptions.Converters.Remove(converter);
                    break;
                }
            }

            JsonSerializer.Serialize(writer, value, typeof(Definition), modifiedOptions);
        }
    }
}
