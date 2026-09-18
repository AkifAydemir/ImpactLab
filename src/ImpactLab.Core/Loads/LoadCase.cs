namespace ImpactLab.Core.Loads;

public sealed class LoadCase
{
    private readonly List<ILoadSource> _loads = [];

    public LoadCase(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public string Id { get; }
    public string Name { get; set; }
    public IReadOnlyList<ILoadSource> Loads => _loads;
    public bool Enabled { get; set; } = true;

    public void Add(ILoadSource source) => _loads.Add(source);

    public ILoadSource Compile() =>
        Enabled ? new CompositeLoadSource(_loads) : new CompositeLoadSource([]);
}
