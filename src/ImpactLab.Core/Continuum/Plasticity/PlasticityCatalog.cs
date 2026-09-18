namespace ImpactLab.Core.Continuum.Plasticity;

public sealed class PlasticityCatalog
{
    private readonly Dictionary<string, MaterialPlasticityProfile> _p = new(
        StringComparer.OrdinalIgnoreCase
    );

    public void Set(MaterialPlasticityProfile p)
    {
        p.Settings.Validate();
        _p[p.MaterialId] = p;
    }

    public MaterialPlasticityProfile? Find(string id) => _p.TryGetValue(id, out var p) ? p : null;

    public static PlasticityCatalog CreateDefaults() => new();
}
