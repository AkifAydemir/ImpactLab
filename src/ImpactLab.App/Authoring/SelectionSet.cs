namespace ImpactLab.App.Authoring;

public sealed class SelectionSet
{
    private readonly HashSet<string> _ids = new(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyCollection<string> Ids => _ids;
    public string? Primary { get; private set; }

    public void Replace(string id)
    {
        _ids.Clear();
        _ids.Add(id);
        Primary = id;
    }

    public void Toggle(string id)
    {
        if (!_ids.Remove(id))
            _ids.Add(id);
        Primary = _ids.Contains(id) ? id : _ids.LastOrDefault();
    }

    public void Clear()
    {
        _ids.Clear();
        Primary = null;
    }
}
