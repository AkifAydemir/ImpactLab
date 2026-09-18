using ImpactLab.Core.Geometry;

namespace ImpactLab.Core.Continuum.Meshing;

public interface IVolumeMesher
{
    string Id { get; }
    TetrahedralMesh Generate(
        GeometrySpec geometry,
        Materials.MaterialDefinition material,
        VolumeMeshingSettings settings,
        CancellationToken ct = default
    );
}
