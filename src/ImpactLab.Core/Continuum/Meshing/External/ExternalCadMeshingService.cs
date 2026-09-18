using ImpactLab.Core.Geometry.Cad;
using ImpactLab.Core.Materials;

namespace ImpactLab.Core.Continuum.Meshing.External;

public sealed class ExternalCadMeshingService
{
    private readonly IGeometryHealer _healer;
    private readonly IExternalTetraMesherAdapter _adapter;

    public ExternalCadMeshingService(
        IExternalTetraMesherAdapter adapter,
        IGeometryHealer? healer = null
    )
    {
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _healer = healer ?? new ManagedGeometryHealer();
    }

    public ExternalCadMeshingOutcome Generate(
        CadModel model,
        MaterialDefinition material,
        VolumeMeshingSettings settings,
        string workingDirectory,
        IReadOnlyDictionary<string, string>? options = null,
        GeometryHealingSettings? healing = null,
        CancellationToken ct = default
    )
    {
        settings.Validate();
        var healed = _healer.Heal(model, healing ?? new(), ct);
        var request = new ExternalTetraMesherRequest(
            healed.Model,
            material,
            settings,
            workingDirectory,
            options ?? new Dictionary<string, string>(StringComparer.Ordinal)
        );
        var meshed = _adapter.Generate(request, ct);
        var quality = meshed.Mesh is null ? null : TetraQualityAnalyzer.Analyze(meshed.Mesh);
        return new(model, healed, meshed, quality);
    }
}
