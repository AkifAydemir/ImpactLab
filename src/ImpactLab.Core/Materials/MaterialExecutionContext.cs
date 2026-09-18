namespace ImpactLab.Core.Materials;

public sealed class MaterialExecutionContext
{
    private readonly Dictionary<string, MaterialExecutionProfile> _execution = new(
        StringComparer.OrdinalIgnoreCase
    );
    private readonly Dictionary<string, CohesiveInterfaceLaw> _cohesive = new(
        StringComparer.OrdinalIgnoreCase
    );

    public MaterialExecutionContext(
        MaterialProfileCatalog profiles,
        ConstitutiveLawRegistry? laws = null
    )
    {
        Profiles = profiles;
        Laws = laws ?? new ConstitutiveLawRegistry();
    }

    public MaterialProfileCatalog Profiles { get; }
    public ConstitutiveLawRegistry Laws { get; }

    public void Set(MaterialExecutionProfile profile)
    {
        profile.Validate();
        _execution[profile.MaterialId] = profile;
    }

    public MaterialExecutionProfile Resolve(string materialId) =>
        _execution.TryGetValue(materialId, out var p)
            ? p
            : new MaterialExecutionProfile(materialId, "rate-temp:tabulated-profile");

    public void SetCohesive(string materialA, string materialB, CohesiveInterfaceLaw law)
    {
        law.Validate();
        _cohesive[Key(materialA, materialB)] = law;
    }

    public CohesiveInterfaceLaw? ResolveCohesive(string materialA, string materialB) =>
        _cohesive.GetValueOrDefault(Key(materialA, materialB));

    private static string Key(string a, string b) =>
        string.CompareOrdinal(a, b) <= 0 ? $"{a}|{b}" : $"{b}|{a}";
}
