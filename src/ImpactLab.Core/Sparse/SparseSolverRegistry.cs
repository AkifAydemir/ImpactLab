namespace ImpactLab.Core.Sparse;

public sealed class SparseSolverRegistry
{
    private readonly Dictionary<string, Func<ISparseLinearSolver>> _factories = new(
        StringComparer.OrdinalIgnoreCase
    );

    public void Register(string id, Func<ISparseLinearSolver> factory) => _factories[id] = factory;

    public ISparseLinearSolver Create(string id) =>
        _factories.TryGetValue(id, out var f) ? f() : throw new KeyNotFoundException(id);

    public ISparseLinearSolver CreatePreferred(bool preferNative = true)
    {
        if (preferNative && _factories.TryGetValue("native-csr", out var nf))
        {
            var n = nf();
            if (n.IsAvailable)
                return n;
        }
        return Create("managed-cg");
    }

    public static SparseSolverRegistry CreateDefault()
    {
        var r = new SparseSolverRegistry();
        r.Register("managed-cg", () => new ConjugateGradientSolver());
        r.Register("native-csr", () => new NativeSparseLinearSolver());
        return r;
    }
}
