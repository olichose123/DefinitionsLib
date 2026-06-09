namespace Olib.Godot.Definitions;

public class DefinitionRegistry : IDefinitionProvider
{
    private readonly Dictionary<Type, Dictionary<string, Definition>> _definitions = new();

    public bool SkipDuplicateDefinitions { get; set; } = false;
    public bool ReplaceDuplicateDefinitions { get; set; } = false;
    public bool ThrowMissingDefinitionExceptions { get; set; } = true;

    public DefinitionRegistry()
    {
    }

    public T? Get<T>(string name) where T : Definition
    {
        if (_definitions.TryGetValue(typeof(T), out var definitionsOfType))
        {
            if (definitionsOfType.TryGetValue(name, out var definition))
            {
                return (T)definition;
            }
        }
        if (ThrowMissingDefinitionExceptions)
        {
            throw new KeyNotFoundException($"No definition of type '{typeof(T).FullName}' with the name '{name}' was found.");
        }
        return default;
    }

    public void Register(Definition definition)
    {
        Type type = definition.GetType();
        if (!_definitions.TryGetValue(type, out var definitionsOfType))
        {
            definitionsOfType = new Dictionary<string, Definition>();
            _definitions[type] = definitionsOfType;
        }

        if (definitionsOfType.ContainsKey(definition.Id))
        {
            if (SkipDuplicateDefinitions && !ReplaceDuplicateDefinitions)
            {
                return;
            }

            if (!ReplaceDuplicateDefinitions)
            {
                throw new InvalidOperationException($"A definition with the ID '{definition.Id}' is already registered.");
            }
        }
        definition._beforeRegistration();
        definitionsOfType[definition.Id] = definition;
    }
}
