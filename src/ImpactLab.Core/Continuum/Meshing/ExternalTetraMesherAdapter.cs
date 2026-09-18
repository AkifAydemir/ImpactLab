using ImpactLab.Core.Geometry;
using ImpactLab.Core.Materials;

namespace ImpactLab.Core.Continuum.Meshing;

public sealed class ExternalTetraMesherAdapter : IVolumeMesher
{
    public ExternalTetraMesherAdapter(string executable) => Executable = executable;

    public string Executable { get; }
    public string Id => "external-tetra";

    public TetrahedralMesh Generate(
        GeometrySpec geometry,
        MaterialDefinition material,
        VolumeMeshingSettings settings,
        CancellationToken ct = default
    ) =>
        throw new NotSupportedException(
            "The process CAD/tetra bridge requires a CadModel and is exposed through ExternalCadMeshingService. "
                + "IVolumeMesher remains geometry-domain neutral and does not serialize arbitrary GeometrySpec instances."
        );
}
