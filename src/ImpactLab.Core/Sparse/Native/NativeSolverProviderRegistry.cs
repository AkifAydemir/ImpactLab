namespace ImpactLab.Core.Sparse.Native;

public sealed class NativeSolverProviderRegistry
{
    private readonly Dictionary<string, NativeSolverProviderDescriptor> _p = new(
        StringComparer.OrdinalIgnoreCase
    );

    public void Register(NativeSolverProviderDescriptor d) => _p[d.Id] = d;

    public IReadOnlyList<NativeSolverProviderDescriptor> All =>
        _p.Values.OrderBy(x => x.DisplayName).ToArray();
}
