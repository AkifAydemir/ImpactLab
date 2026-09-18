namespace ImpactLab.Core.Backends;

public static class SimulationBackendSelector
{
    public static SimulationBackendDescriptor Choose(
        IEnumerable<SimulationBackendDescriptor> backends,
        SimulationBackendCapabilities required,
        string? preferredId = null
    )
    {
        var candidates = backends.Where(x => x.Supports(required)).ToArray();
        if (candidates.Length == 0)
            throw new InvalidOperationException(
                $"No backend supports required capabilities: {required}"
            );
        if (!string.IsNullOrWhiteSpace(preferredId))
        {
            var preferred = candidates.FirstOrDefault(x =>
                string.Equals(x.Id, preferredId, StringComparison.OrdinalIgnoreCase)
            );
            if (preferred is not null)
                return preferred;
        }
        return candidates.OrderByDescending(x => x.Version).ThenBy(x => x.DisplayName).First();
    }
}
