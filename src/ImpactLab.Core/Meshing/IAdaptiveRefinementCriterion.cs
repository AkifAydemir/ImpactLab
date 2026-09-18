using ImpactLab.Core.Geometry;
using ImpactLab.Core.Materials;

namespace ImpactLab.Core.Meshing;

public interface IAdaptiveRefinementCriterion
{
    AdaptiveRefinementDecision Evaluate(
        AdaptiveCell cell,
        GeometrySpec geometry,
        MaterialDefinition baseMaterial,
        IReadOnlyList<MaterialRegion> regions,
        MeshingSettings settings
    );
}
