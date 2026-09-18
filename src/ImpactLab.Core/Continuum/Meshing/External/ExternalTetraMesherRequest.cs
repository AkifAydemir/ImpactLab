using ImpactLab.Core.Geometry.Cad;
using ImpactLab.Core.Materials;

namespace ImpactLab.Core.Continuum.Meshing.External;

public sealed record ExternalTetraMesherRequest(
    CadModel Model,
    MaterialDefinition Material,
    VolumeMeshingSettings Meshing,
    string WorkingDirectory,
    IReadOnlyDictionary<string, string> Options
)
{
    public BoundaryLayerSettings BoundaryLayers => Meshing.BoundaryLayer ?? new();

    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(Model);
        ArgumentNullException.ThrowIfNull(Material);
        Meshing.Validate();
        Material.Validate();
        if (string.IsNullOrWhiteSpace(WorkingDirectory))
            throw new InvalidOperationException("External mesher working directory is required.");
    }
}
