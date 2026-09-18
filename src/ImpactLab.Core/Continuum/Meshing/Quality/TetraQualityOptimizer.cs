using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Meshing.Quality;

public sealed class TetraQualityOptimizer
{
    public TetrahedralMesh Smooth(TetrahedralMesh mesh, TetraSmoothingSettings? settings = null)
    {
        var s = settings ?? new();
        var pos = mesh.Nodes.Select(x => x.Position).ToArray();
        var adjacency = Build(mesh);
        for (var it = 0; it < s.Iterations; it++)
        {
            var next = pos.ToArray();
            for (var i = 0; i < pos.Length; i++)
            {
                if (adjacency[i].Count == 0)
                    continue;
                var avg =
                    adjacency[i].Select(j => pos[j]).Aggregate(Vec3.Zero, (a, b) => a + b)
                    / adjacency[i].Count;
                next[i] = pos[i] * (1 - s.Relaxation) + avg * s.Relaxation;
            }
            pos = next;
        }
        return mesh.WithNodePositions(pos);
    }

    static List<int>[] Build(TetrahedralMesh m)
    {
        var a = Enumerable.Range(0, m.Nodes.Count).Select(_ => new List<int>()).ToArray();
        foreach (var e in m.Elements)
        {
            var ids = new[] { e.A, e.B, e.C, e.D };
            foreach (var i in ids)
            foreach (var j in ids)
                if (i != j && !a[i].Contains(j))
                    a[i].Add(j);
        }
        return a;
    }
}
