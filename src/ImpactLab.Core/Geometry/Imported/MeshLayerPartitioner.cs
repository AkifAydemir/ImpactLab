using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry.Imported;

public static class MeshLayerPartitioner
{
    public static IReadOnlyList<MeshLayer> Split(
        TriangleMeshAsset mesh,
        int layerCount,
        MeshLayerAxis axis = MeshLayerAxis.Z
    )
    {
        ArgumentNullException.ThrowIfNull(mesh);
        if (layerCount is < 1 or > 512)
            throw new ArgumentOutOfRangeException(
                nameof(layerCount),
                "Layer count must be between 1 and 512."
            );

        var minimum = Coordinate(mesh.Bounds.Min, axis);
        var maximum = Coordinate(mesh.Bounds.Max, axis);
        var span = maximum - minimum;
        if (span <= 1e-15)
            throw new InvalidOperationException(
                "Mesh has no extent along the selected layer axis."
            );

        var triangleBuckets = Enumerable
            .Range(0, layerCount)
            .Select(_ => new List<(int A, int B, int C)>())
            .ToArray();
        for (var i = 0; i < mesh.Indices.Count; i += 3)
        {
            var a = mesh.Indices[i];
            var b = mesh.Indices[i + 1];
            var c = mesh.Indices[i + 2];
            var centroid =
                (
                    Coordinate(mesh.Vertices[a], axis)
                    + Coordinate(mesh.Vertices[b], axis)
                    + Coordinate(mesh.Vertices[c], axis)
                ) / 3.0;
            var normalized = Math.Clamp((centroid - minimum) / span, 0.0, 1.0);
            var bucket = Math.Min(layerCount - 1, (int)(normalized * layerCount));
            triangleBuckets[bucket].Add((a, b, c));
        }

        var output = new List<MeshLayer>();
        for (var layerIndex = 0; layerIndex < layerCount; layerIndex++)
        {
            var triangles = triangleBuckets[layerIndex];
            if (triangles.Count == 0)
                continue;
            var vertices = new List<Vec3>();
            var indices = new List<int>(triangles.Count * 3);
            var remap = new Dictionary<int, int>();
            foreach (var triangle in triangles)
            {
                Add(triangle.A);
                Add(triangle.B);
                Add(triangle.C);
            }
            var layerMin = minimum + span * layerIndex / layerCount;
            var layerMax = minimum + span * (layerIndex + 1) / layerCount;
            output.Add(
                new MeshLayer(
                    layerIndex,
                    layerMin,
                    layerMax,
                    new TriangleMeshAsset(
                        $"{mesh.Name}-layer-{layerIndex + 1:D3}",
                        vertices,
                        indices
                    )
                )
            );

            void Add(int sourceIndex)
            {
                if (!remap.TryGetValue(sourceIndex, out var targetIndex))
                {
                    targetIndex = vertices.Count;
                    remap.Add(sourceIndex, targetIndex);
                    vertices.Add(mesh.Vertices[sourceIndex]);
                }
                indices.Add(targetIndex);
            }
        }
        return output;
    }

    private static double Coordinate(Vec3 value, MeshLayerAxis axis) =>
        axis switch
        {
            MeshLayerAxis.X => value.X,
            MeshLayerAxis.Y => value.Y,
            _ => value.Z,
        };
}
