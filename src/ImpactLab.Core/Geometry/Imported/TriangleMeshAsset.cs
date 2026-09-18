using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry.Imported;

public sealed class TriangleMeshAsset
{
    public TriangleMeshAsset(string name, IReadOnlyList<Vec3> vertices, IReadOnlyList<int> indices)
    {
        Name = name;
        Vertices = vertices;
        Indices = indices;
        if (indices.Count == 0 || indices.Count % 3 != 0)
            throw new ArgumentException(
                "Triangle index count must be a non-zero multiple of three."
            );
        if (indices.Any(i => i < 0 || i >= vertices.Count))
            throw new ArgumentException("Triangle index is out of range.");
        var min = new Vec3(vertices.Min(x => x.X), vertices.Min(x => x.Y), vertices.Min(x => x.Z));
        var max = new Vec3(vertices.Max(x => x.X), vertices.Max(x => x.Y), vertices.Max(x => x.Z));
        Bounds = new BoundingBox3(min, max);
    }

    public string Name { get; }
    public IReadOnlyList<Vec3> Vertices { get; }
    public IReadOnlyList<int> Indices { get; }
    public BoundingBox3 Bounds { get; }
    public int TriangleCount => Indices.Count / 3;
    public IEnumerable<Triangle3> Triangles
    {
        get
        {
            for (var i = 0; i < Indices.Count; i += 3)
                yield return new Triangle3(
                    Vertices[Indices[i]],
                    Vertices[Indices[i + 1]],
                    Vertices[Indices[i + 2]]
                );
        }
    }
}
