namespace ImpactLab.Core.Materials;

public sealed class ConstitutiveLawRegistry
{
    private readonly Dictionary<string, IConstitutiveLaw> _laws = new(
        StringComparer.OrdinalIgnoreCase
    );

    public ConstitutiveLawRegistry()
    {
        Register(new LegacyBilinearConstitutiveLaw());
        Register(new TabulatedConstitutiveLaw());
        Register(new RateTemperatureAdjustedLaw(new TabulatedConstitutiveLaw()));
    }

    public void Register(IConstitutiveLaw law) => _laws[law.Id] = law;

    public IConstitutiveLaw Get(string id) =>
        _laws.TryGetValue(id, out var law)
            ? law
            : throw new KeyNotFoundException($"Unknown constitutive law: {id}");

    public IReadOnlyCollection<string> Ids => _laws.Keys;
}
