using ImpactLab.Core.Geometry;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Meshing;

public static class StructuredMeshBuilder
{
    private static readonly (int X, int Y, int Z)[] PositiveNeighborOffsets =
    [
        (1, 0, 0),
        (0, 1, 0),
        (0, 0, 1),
        (1, 1, 0),
        (1, -1, 0),
        (1, 0, 1),
        (1, 0, -1),
        (0, 1, 1),
        (0, 1, -1),
        (1, 1, 1),
        (1, 1, -1),
        (1, -1, 1),
        (1, -1, -1),
    ];

    public static SimulationMesh Build(
        string partId,
        GeometrySpec geometry,
        MaterialDefinition baseMaterial,
        double cellSize,
        IReadOnlyList<MaterialRegion>? materialRegions = null,
        MaterialInterfaceTable? interfaceTable = null
    )
    {
        if (string.IsNullOrWhiteSpace(partId))
            throw new ArgumentException("Part id cannot be empty.", nameof(partId));
        baseMaterial.Validate();
        if (cellSize <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(cellSize));
        var interfaces = interfaceTable ?? new MaterialInterfaceTable();
        var regions = (materialRegions ?? []).OrderByDescending(x => x.Priority).ToArray();
        foreach (var region in regions)
            region.Material.Validate();
        var bounds = geometry.Bounds;
        var size = bounds.Size;
        var countX = Math.Max(1, (int)Math.Ceiling(size.X / cellSize));
        var countY = Math.Max(1, (int)Math.Ceiling(size.Y / cellSize));
        var countZ = Math.Max(1, (int)Math.Ceiling(size.Z / cellSize));
        var nodes = new List<MeshNode>(countX * countY * countZ);
        var localMap = new Dictionary<(int X, int Y, int Z), int>();
        var globalMap = new Dictionary<(string PartId, int X, int Y, int Z), int>();
        var cellVolume = cellSize * cellSize * cellSize;
        for (var z = 0; z < countZ; z++)
        {
            for (var y = 0; y < countY; y++)
            {
                for (var x = 0; x < countX; x++)
                {
                    var point = new Vec3(
                        bounds.Min.X + (x + 0.5) * cellSize,
                        bounds.Min.Y + (y + 0.5) * cellSize,
                        bounds.Min.Z + (z + 0.5) * cellSize
                    );
                    if (!geometry.Contains(point))
                        continue;
                    var material = SelectMaterial(point, baseMaterial, regions);
                    var nodeMass = material.DensityKgPerM3 * cellVolume;
                    var id = nodes.Count;
                    nodes.Add(
                        new MeshNode(id, partId, x, y, z, point, nodeMass, cellSize, material)
                    );
                    localMap[(x, y, z)] = id;
                    globalMap[(partId, x, y, z)] = id;
                }
            }
        }
        if (nodes.Count == 0)
            throw new InvalidOperationException(
                "Geometry produced an empty mesh. Reduce cell size."
            );
        var springs = BuildSprings(partId, nodes, localMap, cellSize, interfaces);
        var part = new MeshPartRange(partId, 0, nodes.Count, 0, springs.Count, bounds);
        return new SimulationMesh(nodes, springs, cellSize, globalMap, [part]);
    }

    private static MaterialDefinition SelectMaterial(
        in Vec3 point,
        MaterialDefinition baseMaterial,
        IReadOnlyList<MaterialRegion> regions
    )
    {
        foreach (var region in regions)
        {
            if (region.Geometry.Contains(point))
                return region.Material;
        }
        return baseMaterial;
    }

    private static IReadOnlyList<MeshSpring> BuildSprings(
        string partId,
        IReadOnlyList<MeshNode> nodes,
        IReadOnlyDictionary<(int X, int Y, int Z), int> map,
        double cellSize,
        MaterialInterfaceTable interfaces
    )
    {
        var springs = new List<MeshSpring>(nodes.Count * 8);
        foreach (var node in nodes)
        {
            foreach (var offset in PositiveNeighborOffsets)
            {
                var key = (node.GridX + offset.X, node.GridY + offset.Y, node.GridZ + offset.Z);
                if (!map.TryGetValue(key, out var neighborId))
                    continue;
                var neighbor = nodes[neighborId];
                var restVector = neighbor.RestPosition - node.RestPosition;
                var restLength = restVector.Length;
                var diagonalFactor = restLength / cellSize;
                var area = cellSize * cellSize / Math.Max(1.0, diagonalFactor);
                var material = MaterialMixer.Mix(node.Material, neighbor.Material);
                var materialInterface = interfaces.Resolve(node.Material, neighbor.Material);
                springs.Add(
                    new MeshSpring(
                        partId,
                        node.Id,
                        neighbor.Id,
                        restLength,
                        area,
                        material,
                        materialInterface
                    )
                );
            }
        }
        return springs;
    }
}
