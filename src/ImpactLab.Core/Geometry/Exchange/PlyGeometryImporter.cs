using System.Globalization;
using ImpactLab.Core.Geometry.Imported;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry.Exchange;

public sealed class PlyGeometryImporter : IGeometryExchangeImporter
{
    public string Id => "ply-ascii";
    public IReadOnlyCollection<string> Extensions => [".ply"];

    public TriangleMeshAsset Import(string path)
    {
        using var r = new StreamReader(path);
        if (r.ReadLine()?.Trim() != "ply")
            throw new InvalidDataException("Not a PLY file.");
        int nv = 0,
            nf = 0;
        string? line;
        while ((line = r.ReadLine()) is not null && line.Trim() != "end_header")
        {
            var s = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (s.Length >= 3 && s[0] == "element" && s[1] == "vertex")
                nv = int.Parse(s[2], CultureInfo.InvariantCulture);
            if (s.Length >= 3 && s[0] == "element" && s[1] == "face")
                nf = int.Parse(s[2], CultureInfo.InvariantCulture);
        }
        var v = new Vec3[nv];
        for (var i = 0; i < nv; i++)
        {
            var s = r.ReadLine()!.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            v[i] = new(
                double.Parse(s[0], CultureInfo.InvariantCulture),
                double.Parse(s[1], CultureInfo.InvariantCulture),
                double.Parse(s[2], CultureInfo.InvariantCulture)
            );
        }
        var idx = new List<int>();
        for (var i = 0; i < nf; i++)
        {
            var s = r.ReadLine()!.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var n = int.Parse(s[0], CultureInfo.InvariantCulture);
            var face = s.Skip(1)
                .Take(n)
                .Select(value => int.Parse(value, CultureInfo.InvariantCulture))
                .ToArray();
            for (var k = 1; k < n - 1; k++)
            {
                idx.Add(face[0]);
                idx.Add(face[k]);
                idx.Add(face[k + 1]);
            }
        }
        return new(Path.GetFileNameWithoutExtension(path), v, idx.ToArray());
    }
}
