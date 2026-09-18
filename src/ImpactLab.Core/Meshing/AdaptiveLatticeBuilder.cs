using ImpactLab.Core.Geometry;
using ImpactLab.Core.Materials;

namespace ImpactLab.Core.Meshing;

public static class AdaptiveLatticeBuilder
{
    public static SimulationMesh Build(
        string partId,
        GeometrySpec geometry,
        MaterialDefinition baseMaterial,
        MeshingSettings settings,
        IReadOnlyList<MaterialRegion>? materialRegions = null,
        MaterialInterfaceTable? interfaceTable = null
    )
    {
        var regions = (materialRegions ?? []).OrderByDescending(x => x.Priority).ToArray();
        var interfaces = interfaceTable ?? new MaterialInterfaceTable();
        var plan = new AdaptiveMeshPlanner().Plan(geometry, baseMaterial, regions, settings);
        var nodes = new List<MeshNode>();
        var map = new Dictionary<(string, int, int, int), int>();
        var seq = 0;
        foreach (var c in plan.Leaves)
        {
            if (!geometry.Contains(c.Center))
                continue;
            var mat = Resolve(c.Center, baseMaterial, regions);
            var cell = c.CharacteristicSize;
            var id = nodes.Count;
            nodes.Add(
                new MeshNode(
                    id,
                    partId,
                    seq++,
                    0,
                    0,
                    c.Center,
                    mat.DensityKgPerM3 * c.Volume,
                    cell,
                    mat
                )
            );
            map[(partId, nodes[^1].GridX, 0, 0)] = id;
        }
        if (nodes.Count == 0)
            throw new InvalidOperationException(
                "Adaptive occupied-cell sampling produced no nodes."
            );
        var springs = AdaptiveSpringBuilder.Build(partId, nodes, interfaces);
        var part = new MeshPartRange(partId, 0, nodes.Count, 0, springs.Count, geometry.Bounds);
        var boundaries = new Dictionary<string, IReadOnlyList<int>>(
            StringComparer.OrdinalIgnoreCase
        )
        {
            { partId, AdaptiveBoundaryClassifier.FindBoundaryNodes(geometry, nodes) },
        };
        return new SimulationMesh(
            nodes,
            springs,
            plan.MinimumCellSize,
            map,
            [part],
            MeshTopologyKind.AdaptiveLattice,
            boundaries
        );
    }

    private static MaterialDefinition Resolve(
        in ImpactLab.Core.Mathematics.Vec3 p,
        MaterialDefinition baseMaterial,
        IReadOnlyList<MaterialRegion> regions
    )
    {
        foreach (var r in regions)
            if (r.Geometry.Contains(p))
                return r.Material;
        return baseMaterial;
    }
}
