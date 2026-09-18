using System.Globalization;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry.Imported;

public sealed class ObjMeshImporter : IRawMeshImporter
{
    public IReadOnlyCollection<string> Extensions { get; } = new[] { ".obj" };

    public TriangleMeshAsset Import(string path)
    {
        var vertices = new List<Vec3>();
        var indices = new List<int>();
        foreach (var raw in File.ReadLines(path))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;
            var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts[0] == "v" && parts.Length >= 4)
                vertices.Add(new Vec3(D(parts[1]), D(parts[2]), D(parts[3])));
            else if (parts[0] == "f" && parts.Length >= 4)
            {
                var face = parts.Skip(1).Select(x => ParseIndex(x, vertices.Count)).ToArray();
                for (var i = 1; i + 1 < face.Length; i++)
                {
                    indices.Add(face[0]);
                    indices.Add(face[i]);
                    indices.Add(face[i + 1]);
                }
            }
        }
        return new TriangleMeshAsset(Path.GetFileNameWithoutExtension(path), vertices, indices);
    }

    private static double D(string value) =>
        double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);

    private static int ParseIndex(string token, int count)
    {
        var i = int.Parse(token.Split('/')[0], CultureInfo.InvariantCulture);
        return i > 0 ? i - 1 : count + i;
    }
}
