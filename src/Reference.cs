using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Olib.Godot.Definitions")]
namespace Olib.Godot.Definitions;

public class Reference<T> : IReference<T> where T : Definition
{
    private readonly IDefinitionProvider _definitionProvider;
    public string? Id { get; set; }

    T? _cachedValue;

    public Reference(string? id, IDefinitionProvider definitionProvider)
    {
        Id = id;
        _definitionProvider = definitionProvider;
    }

    public T? Get()
    {
        if (Id == null)
            return default;

        if (_cachedValue != null && _cachedValue.Id == Id)
            return _cachedValue;

        if (_cachedValue == null)
        {
            _cachedValue = _definitionProvider.Get<T>(Id);
        }

        return _cachedValue;
    }
}
