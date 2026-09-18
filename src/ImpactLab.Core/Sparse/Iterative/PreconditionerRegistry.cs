namespace ImpactLab.Core.Sparse.Iterative;

public sealed class PreconditionerRegistry
{
    private readonly Dictionary<string, Func<ISparsePreconditioner>> _f = new(
        StringComparer.OrdinalIgnoreCase
    );

    public void Register(string id, Func<ISparsePreconditioner> f) => _f[id] = f;

    public ISparsePreconditioner Create(string id) =>
        _f.TryGetValue(id, out var f) ? f() : throw new KeyNotFoundException(id);

    public static PreconditionerRegistry CreateDefault()
    {
        var r = new PreconditionerRegistry();
        r.Register("jacobi", () => new JacobiPreconditioner());
        r.Register("block-jacobi", () => new BlockJacobiPreconditioner());
        return r;
    }
}
