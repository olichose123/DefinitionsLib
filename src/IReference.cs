namespace Olib.Godot.Definitions;

public interface IReference<T>
{
    public string? Id { get; set; }

    public T? Get();
}
