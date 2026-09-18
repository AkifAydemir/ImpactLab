namespace ImpactLab.App.Accessibility;

public sealed class KeyboardNavigationMap
{
    private readonly Dictionary<string, string> _map = new(StringComparer.OrdinalIgnoreCase);

    public void Set(string command, string gesture) => _map[command] = gesture;

    public string? Get(string command) => _map.TryGetValue(command, out var x) ? x : null;
}
