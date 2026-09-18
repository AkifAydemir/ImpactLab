namespace ImpactLab.Core.Meshing;

public static class SimulationMeshAssembler
{
    public static SimulationMesh Combine(IReadOnlyList<SimulationMesh> meshes)
    {
        if (meshes.Count == 0)
            throw new ArgumentException("At least one mesh is required.", nameof(meshes));
        var nodes = new List<MeshNode>();
        var springs = new List<MeshSpring>();
        var parts = new List<MeshPartRange>();
        var map = new Dictionary<(string, int, int, int), int>();
        var boundaries = new Dictionary<string, IReadOnlyList<int>>(
            StringComparer.OrdinalIgnoreCase
        );
        var topology = meshes.Any(x => x.TopologyKind == MeshTopologyKind.AdaptiveLattice)
            ? MeshTopologyKind.AdaptiveLattice
            : MeshTopologyKind.UniformStructured;
        var representative = meshes.Min(x => x.CellSize);
        foreach (var mesh in meshes)
        foreach (var sourcePart in mesh.Parts)
        {
            var nodeStart = nodes.Count;
            var springStart = springs.Count;
            var remap = new Dictionary<int, int>();
            for (var i = sourcePart.NodeStart; i < sourcePart.NodeEndExclusive; i++)
            {
                var s = mesh.Nodes[i];
                var newId = nodes.Count;
                remap[s.Id] = newId;
                nodes.Add(
                    new MeshNode(
                        newId,
                        s.PartId,
                        s.GridX,
                        s.GridY,
                        s.GridZ,
                        s.RestPosition,
                        s.MassKg,
                        s.CellSize,
                        s.Material
                    )
                );
                map[(s.PartId, s.GridX, s.GridY, s.GridZ)] = newId;
            }
            for (var i = sourcePart.SpringStart; i < sourcePart.SpringEndExclusive; i++)
            {
                var s = mesh.Springs[i];
                springs.Add(
                    new MeshSpring(
                        s.PartId,
                        remap[s.NodeA],
                        remap[s.NodeB],
                        s.RestLength,
                        s.EffectiveAreaM2,
                        s.Material,
                        s.Interface
                    )
                    {
                        Damage = s.Damage,
                    }
                );
            }
            parts.Add(
                new MeshPartRange(
                    sourcePart.PartId,
                    nodeStart,
                    nodes.Count - nodeStart,
                    springStart,
                    springs.Count - springStart,
                    sourcePart.Bounds
                )
            );
            if (mesh.TryGetBoundaryNodes(sourcePart.PartId, out var ids))
                boundaries[sourcePart.PartId] = ids.Select(x => remap[x]).ToArray();
        }
        return new SimulationMesh(nodes, springs, representative, map, parts, topology, boundaries);
    }
}
