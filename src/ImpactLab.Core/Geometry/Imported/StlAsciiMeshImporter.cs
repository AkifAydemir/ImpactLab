using System.Globalization;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry.Imported;

public sealed class StlAsciiMeshImporter : IRawMeshImporter
{
    public IReadOnlyCollection<string> Extensions { get; } = new[] { ".stl" };

    public TriangleMeshAsset Import(string path)
    {
        var vertices = new List<Vec3>();
        var indices = new List<int>();
        foreach (var raw in File.ReadLines(path))
        {
            var line = raw.Trim();
            if (!line.StartsWith("vertex ", StringComparison.OrdinalIgnoreCase))
                continue;
            var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4)
                continue;
            vertices.Add(new Vec3(D(parts[1]), D(parts[2]), D(parts[3])));
            indices.Add(vertices.Count - 1);
        }
        return new TriangleMeshAsset(Path.GetFileNameWithoutExtension(path), vertices, indices);
    }

    private static double D(string value) =>
        double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
}
