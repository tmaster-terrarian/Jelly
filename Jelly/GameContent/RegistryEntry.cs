namespace Jelly.GameContent;

public abstract class RegistryEntry
{
    public string Name { get; set; }

    public RegistryEntry()
    {
        Name = GetType().Name;
    }
}
