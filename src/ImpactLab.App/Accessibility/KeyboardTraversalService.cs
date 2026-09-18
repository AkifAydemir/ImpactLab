namespace ImpactLab.App.Accessibility;

public sealed class KeyboardTraversalService
{
    private readonly List<string> _order = [];
    public string? Current { get; private set; }

    public void Configure(IEnumerable<string> automationIds)
    {
        _order.Clear();
        _order.AddRange(
            automationIds
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
        );
        Current = _order.FirstOrDefault();
    }

    public string? Move(bool reverse = false)
    {
        if (_order.Count == 0)
            return null;
        var i = Current is null
            ? 0
            : _order.FindIndex(x => x.Equals(Current, StringComparison.OrdinalIgnoreCase));
        i = (i + (reverse ? -1 : 1) + _order.Count) % _order.Count;
        return Current = _order[i];
    }
}
