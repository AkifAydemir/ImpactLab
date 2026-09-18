namespace ImpactLab.App.Authoring;

public sealed class SelectionService
{
    public SelectionSet Current { get; } = new();
    public event EventHandler? Changed;

    public void Select(string id, bool additive = false)
    {
        if (additive)
            Current.Toggle(id);
        else
            Current.Replace(id);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        Current.Clear();
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
