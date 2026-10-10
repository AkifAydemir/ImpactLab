namespace ImpactLab.Core.Fracture;

public sealed class FragmentLineageTracker
{
    private readonly Dictionary<string, FragmentLineageRecord> _records = new(
        StringComparer.Ordinal
    );
    private IReadOnlyList<FragmentClusterSnapshot> _previous = [];

    public IReadOnlyCollection<FragmentLineageRecord> Records => _records.Values;

    public IReadOnlyList<FragmentLineageRecord> Observe(
        IReadOnlyList<FragmentClusterSnapshot> fragments,
        int step,
        double timeSeconds
    )
    {
        var births = new List<FragmentLineageRecord>();
        foreach (var fragment in fragments)
        {
            if (_records.ContainsKey(fragment.Id))
                continue;
            var parent = FindParent(fragment);
            var generation = parent is null ? 0 : parent.Generation + 1;
            var record = new FragmentLineageRecord(
                fragment.Id,
                fragment.PartId,
                parent?.FragmentId,
                generation,
                step,
                timeSeconds,
                fragment.NodeIds.ToArray()
            );
            _records.Add(record.FragmentId, record);
            births.Add(record);
        }
        _previous = fragments.ToArray();
        return births;
    }

    private FragmentLineageRecord? FindParent(FragmentClusterSnapshot fragment)
    {
        var nodes = fragment.NodeIds.ToHashSet();
        var candidate = _previous
            .Where(x =>
                string.Equals(x.PartId, fragment.PartId, StringComparison.OrdinalIgnoreCase)
            )
            .Select(x => new { Fragment = x, Overlap = x.NodeIds.Count(nodes.Contains) })
            .Where(x => x.Overlap > 0)
            .OrderByDescending(x => x.Overlap)
            .ThenByDescending(x => x.Fragment.MassKg)
            .ThenBy(x => x.Fragment.Id, StringComparer.Ordinal)
            .FirstOrDefault()
            ?.Fragment;
        return candidate is not null && _records.TryGetValue(candidate.Id, out var record)
            ? record
            : null;
    }
}
