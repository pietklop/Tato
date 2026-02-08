namespace Core;

public class Bookmaker
{
    public string Name { get; }

    public Bookmaker(string name)
    {
        Name = name;
    }

    public override string ToString() => Name;
}