namespace Olib.Godot.Definitions;

public interface IDefinitionProvider
{
    public T? Get<T>(string name) where T : Definition;
    public void Register(Definition definition);
}
