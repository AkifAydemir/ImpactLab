using ImpactLab.Core.Geometry.Imported;

namespace ImpactLab.Core.Geometry.Repair;

public static class BoundaryLoopFinder
{
    public static IReadOnlyList<int[]> Find(TriangleMeshAsset a)
    {
        var edge = new Dictionary<(int, int), int>();
        for (var i = 0; i < a.Indices.Count; i += 3)
        {
            var t = new[] { a.Indices[i], a.Indices[i + 1], a.Indices[i + 2] };
            for (var k = 0; k < 3; k++)
            {
                var x = t[k];
                var y = t[(k + 1) % 3];
                var q = x < y ? (x, y) : (y, x);
                edge[q] = edge.TryGetValue(q, out var c) ? c + 1 : 1;
            }
        }
        var adj = new Dictionary<int, List<int>>();
        foreach (var e in edge.Where(x => x.Value == 1).Select(x => x.Key))
        {
            if (!adj.TryGetValue(e.Item1, out var a1))
                adj[e.Item1] = a1 = [];
            if (!adj.TryGetValue(e.Item2, out var a2))
                adj[e.Item2] = a2 = [];
            a1.Add(e.Item2);
            a2.Add(e.Item1);
        }
        var loops = new List<int[]>();
        var seen = new HashSet<(int, int)>();
        foreach (var start in adj.Keys)
        {
            foreach (var next in adj[start])
            {
                var ek = start < next ? (start, next) : (next, start);
                if (seen.Contains(ek))
                    continue;
                var loop = new List<int> { start };
                var prev = start;
                var cur = next;
                while (true)
                {
                    loop.Add(cur);
                    seen.Add(prev < cur ? (prev, cur) : (cur, prev));
                    var candidates = adj[cur].Where(x => x != prev).ToArray();
                    if (candidates.Length == 0 || cur == start)
                        break;
                    var n = candidates.FirstOrDefault(x =>
                        !seen.Contains(cur < x ? (cur, x) : (x, cur))
                    );
                    if (n == 0 && candidates.All(x => x != 0))
                        n = candidates[0];
                    prev = cur;
                    cur = n;
                    if (loop.Count > adj.Count + 2)
                        break;
                }
                if (loop.Count >= 3 && loop[^1] == start)
                    loops.Add(loop.ToArray());
            }
        }
        return loops;
    }
}
