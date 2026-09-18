using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Spatial;

namespace ImpactLab.Core.Meshing;

public static class AdaptiveSpringBuilder
{
    public static IReadOnlyList<MeshSpring> Build(
        string partId,
        IReadOnlyList<MeshNode> nodes,
        MaterialInterfaceTable interfaces
    )
    {
        var springs = new List<MeshSpring>();
        if (nodes.Count < 2)
            return springs;
        var maxCell = nodes.Max(x => x.CellSize);
        var hash = new SpatialHash3(maxCell * 1.5);
        foreach (var n in nodes)
            hash.Insert(n.Id, n.RestPosition);
        var candidates = new List<int>();
        var seen = new HashSet<long>();
        foreach (var a in nodes)
        {
            hash.Query(a.RestPosition, a.CellSize * 2.7 + maxCell, candidates);
            foreach (var id in candidates)
            {
                if (id == a.Id)
                    continue;
                var b = nodes[id];
                var key = ((long)Math.Min(a.Id, b.Id) << 32) | (uint)Math.Max(a.Id, b.Id);
                if (!seen.Add(key))
                    continue;
                var d = (b.RestPosition - a.RestPosition).Length;
                var reach = 1.85 * (a.CellSize + b.CellSize) * 0.5;
                if (d > reach || d < 1e-12)
                    continue;
                var ratio = d / Math.Max(Math.Min(a.CellSize, b.CellSize), 1e-12);
                if (ratio > 2.6)
                    continue;
                var area = Math.Pow(Math.Min(a.CellSize, b.CellSize), 2) / Math.Max(1.0, ratio);
                var mat = MaterialMixer.Mix(a.Material, b.Material);
                springs.Add(
                    new MeshSpring(
                        partId,
                        a.Id,
                        b.Id,
                        d,
                        area,
                        mat,
                        interfaces.Resolve(a.Material, b.Material)
                    )
                );
            }
        }
        return springs;
    }
}
