using System.Text.Json.Serialization;

namespace Olib.Godot.Definitions;

public class Definition
{
    public string Id { get; set; }

    [JsonIgnore]
    public string? OriginFilePath { get; set; }

    [JsonConstructor]
    public Definition(string id)
    {
        Id = id;
    }

    public virtual void _beforeRegistration()
    {

    }

    public override string ToString()
    {
        return $"{GetType().Name}({Id})";
    }
}
