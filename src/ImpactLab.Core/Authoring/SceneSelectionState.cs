namespace ImpactLab.Core.Authoring;

public sealed class SceneSelectionState
{
    private readonly HashSet<string> _ids = new(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyCollection<string> SelectedIds => _ids;
    public string? PrimaryId { get; private set; }

    public void SelectSingle(string id)
    {
        _ids.Clear();
        _ids.Add(id);
        PrimaryId = id;
    }

    public void Toggle(string id)
    {
        if (!_ids.Add(id))
            _ids.Remove(id);
        PrimaryId = _ids.Contains(id) ? id : _ids.FirstOrDefault();
    }

    public void Clear()
    {
        _ids.Clear();
        PrimaryId = null;
    }
}
