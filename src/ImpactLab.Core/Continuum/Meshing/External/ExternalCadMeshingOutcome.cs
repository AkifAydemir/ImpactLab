using ImpactLab.Core.Geometry.Cad;

namespace ImpactLab.Core.Continuum.Meshing.External;

public sealed record ExternalCadMeshingOutcome(
    CadModel InputModel,
    GeometryHealingReport Healing,
    ExternalTetraMesherResult Mesher,
    TetraMeshQualityReport? Quality
)
{
    public bool Success =>
        Mesher.Success && Mesher.Mesh is not null && Mesher.Validation?.Passed == true;
}
