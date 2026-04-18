using System.Text.Json;
using System.Text.Json.Serialization;

namespace Definitions
{
    /// <summary>
    /// A definition is a named object that can be registered and retrieved by name. Definitions can be deserialized from JSON and can also be serialized to JSON. The class provides static methods for retrieving definitions by name and for retrieving all definitions of a specific type. It also provides events for when a definition is registered and when a new definition type is encountered. The class implements the IJsonOnDeserialized interface to allow for custom logic to be executed after deserialization, such as registering the definition.
    /// </summary>
    public class Definition : IJsonOnDeserialized
    {
        /// <summary>
        /// Event that is raised when a definition is registered. The event handler receives the registered definition and its type as arguments. This allows for external code to react to the registration of definitions, such as by performing additional validation, logging, or triggering other actions based on the newly registered definition.
        /// </summary>
        public static event EventHandler<DefinitionEventargs>? DefinitionRegistered;

        /// <summary>
        /// Event that is raised when a new definition type is encountered during registration. The event handler receives the registered definition and its type as arguments. This allows for external code to react to the discovery of new definition types, such as by performing additional setup, logging, or triggering other actions based on the newly discovered definition type.
        /// </summary>
        public static event EventHandler<DefinitionEventargs>? NewDefinitionTypeEncountered;

        /// <summary>
        /// Event that is raised when a definition is replaced during registration. This occurs when a definition is registered with a name that already exists for that type, and the SkipDuplicateDefinitions property is false. The event handler receives the new definition that is being registered and its type as arguments. This allows for external code to react to the replacement of definitions, such as by performing additional validation, logging, or triggering other actions based on the new definition that is replacing the existing one. Note that the existing definition that is being replaced is not provided in the event arguments, so if you need to know about the existing definition, you would need to check for it in the DefinitionRegistered event before it gets replaced.
        /// </summary>
        public static event EventHandler<DefinitionEventargs>? DefinitionReplaced;

        public static JsonSerializerOptions JsonSerializerOptions { get; private set; } = new JsonSerializerOptions
        {
            WriteIndented = true,
            IgnoreReadOnlyFields = true,
            TypeInfoResolver = new DefinitionTypeResolver(),
            Converters = { new ReferenceJsonConverter() },
        };

        /// <summary>
        /// If true, when a definition is registered with a name that already exists for that type, it will be ignored instead of throwing an exception. This allows for definitions to be overridden by later definitions in the same file or in different files, without needing to worry about the order of registration. However, it also means that if there are duplicate definitions with the same name and type, only the first one will be registered and the others will be ignored, which could lead to unexpected behavior if not used carefully.
        /// </summary>
        public static bool SkipDuplicateDefinitions { get; set; } = false;

        static Dictionary<Type, Dictionary<string, Definition>> registeredDefinitions = new();

        /// <summary>
        /// Gets a definition by name. Throws an exception if the definition does not exist.
        /// </summary>
        /// <typeparam name="T">The type of the definition.</typeparam>
        /// <param name="name">The name of the definition.</param>
        /// <returns>The definition of the specified type and name.</returns>
        /// <exception cref="DefinitionException">Thrown if the definition does not exist.</exception>
        public static T Get<T>(string name) where T : Definition
        {
            if (registeredDefinitions.TryGetValue(typeof(T), out var definitions))
            {
                if (definitions.TryGetValue(name, out var definition))
                {
                    return (T)definition;
                }
            }
            throw new DefinitionException($"Definition with name {name} does not exist for type {typeof(T).Name}");
        }

        /// <summary>
        /// Gets all definitions of a specific type. Returns an empty list if no definitions of that type exist.
        /// </summary>
        /// <typeparam name="T">The type of the definitions.</typeparam>
        /// <returns>A list of definitions of the specified type.</returns>
        public static List<T> GetAll<T>() where T : Definition
        {
            if (registeredDefinitions.TryGetValue(typeof(T), out var definitions))
            {
                return definitions.Values.Cast<T>().ToList();
            }
            return new List<T>();
        }

        /// <summary>
        /// Parses a definition or a list of definitions from a JSON string. The path is used for error reporting and is stored in the OriginFilePath property of the definition(s).
        /// </summary>
        /// <param name="json">The JSON string to parse.</param>
        /// <param name="path">The path of the JSON file, used for error reporting.</param>
        /// <param name="definitions">The output list of definitions.</param>
        public static void Parse(string json, string path, out List<Definition>? definitions)
        {
            if (IsList(json))
            {
                definitions = DeserializeList(json, path);
            }
            else
            {
                definitions = new List<Definition> { Deserialize<Definition>(json, path) };
            }
        }

        static void Register(Definition definition)
        {
            if (!registeredDefinitions.TryGetValue(definition.GetType(), out var definitions))
            {
                definitions = new Dictionary<string, Definition>();
                registeredDefinitions.Add(definition.GetType(), definitions);
                NewDefinitionTypeEncountered?.Invoke(definition, new DefinitionEventargs(definition));
            }
            if (definitions.ContainsKey(definition.Name))
            {
                if (SkipDuplicateDefinitions)
                {
                    return;
                }
                else
                {
                    DefinitionReplaced?.Invoke(definition, new DefinitionEventargs(definition));
                }
            }
            definitions.Add(definition.Name, definition);
            DefinitionRegistered?.Invoke(definition, new DefinitionEventargs(definition));
        }

        static T Deserialize<T>(string json, string path) where T : Definition
        {
            var definition = JsonSerializer.Deserialize<T>(json, JsonSerializerOptions);
            if (definition == null)
            {
                throw new DefinitionException($"Failed to deserialize definition from {path}");
            }
            definition.OriginFilePath = path;
            return definition;
        }

        static List<Definition> DeserializeList(string json, string path)
        {
            var definitionList = JsonSerializer.Deserialize<List<Definition>>(json, JsonSerializerOptions);
            if (definitionList != null)
            {
                foreach (var def in definitionList)
                {
                    def.OriginFilePath = path;
                }
            }
            else
            {
                throw new DefinitionException($"Failed to deserialize definition list from {path}");
            }
            return definitionList;
        }

        static bool IsList(string json)
        {
            var jsonDoc = JsonDocument.Parse(json);
            return jsonDoc.RootElement.ValueKind == JsonValueKind.Array;
        }

        public required string Name { get; set; }

        [JsonIgnore]
        public string? OriginFilePath;

        [JsonConstructor]
        protected Definition()
        {

        }

        public Definition(string name)
        {
            Name = name;
            BeforeRegistration();
            Register(this);
        }

        protected virtual void BeforeRegistration()
        {

        }

        void IJsonOnDeserialized.OnDeserialized()
        {
            BeforeRegistration();
            Register(this);
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this, JsonSerializerOptions);
        }

        public override string ToString()
        {
            return $"{GetType().Name}({Name})";
        }
    }

    public class DefinitionEventargs : EventArgs
    {
        public Definition Definition { get; }
        public Type Type { get; }

        public DefinitionEventargs(Definition definition)
        {
            Definition = definition;
            Type = definition.GetType();
        }
    }

    public class DefinitionException : Exception
    {
        public DefinitionException(string message) : base(message) { }
    }
}
