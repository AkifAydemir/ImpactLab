namespace ImpactLab.Core.Continuum.FiniteStrain;

public sealed class FiniteStrainLawRegistry
{
    private readonly Dictionary<string, Func<IFiniteStrainConstitutiveLaw>> _f = new(
        StringComparer.OrdinalIgnoreCase
    );

    public void Register(string id, Func<IFiniteStrainConstitutiveLaw> factory) => _f[id] = factory;

    public IFiniteStrainConstitutiveLaw Create(string id) =>
        _f.TryGetValue(id, out var f) ? f() : throw new KeyNotFoundException(id);

    public static FiniteStrainLawRegistry CreateDefault()
    {
        var r = new FiniteStrainLawRegistry();
        r.Register("neo-hookean", () => new NeoHookeanLaw());
        r.Register("finite-j2", () => new FiniteStrainJ2Law());
        return r;
    }
}
