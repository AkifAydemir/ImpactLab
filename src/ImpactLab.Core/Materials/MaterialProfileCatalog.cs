namespace ImpactLab.Core.Materials;

public sealed class MaterialProfileCatalog
{
    private readonly Dictionary<string, MaterialProfile> _profiles = new(
        StringComparer.OrdinalIgnoreCase
    );

    public MaterialProfileCatalog(MaterialCatalog legacy)
    {
        foreach (var entry in legacy.Entries)
            _profiles[entry.Definition.Id] = new MaterialProfile(entry.Definition)
            {
                Provenance = new MaterialProvenance(
                    entry.IsBuiltIn ? "ImpactLab built-in generic dataset" : "User material"
                ),
                Notes = entry.Notes,
            };
    }

    public IReadOnlyCollection<MaterialProfile> Profiles => _profiles.Values;

    public MaterialProfile Get(string id) =>
        _profiles.TryGetValue(id, out var p)
            ? p
            : throw new KeyNotFoundException($"Unknown material profile: {id}");

    public void Upsert(MaterialProfile profile)
    {
        profile.Validate();
        _profiles[profile.Definition.Id] = profile;
    }
}
