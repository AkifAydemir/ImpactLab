namespace ImpactLab.Core.Continuum.Contact;

public sealed class SurfaceAdjacencyMap
{
    private readonly HashSet<(int, int)> _adj = [];

    public SurfaceAdjacencyMap(IReadOnlyList<SurfaceTriangle> t)
    {
        var byNode = new Dictionary<int, List<int>>();
        for (var i = 0; i < t.Count; i++)
            foreach (var n in t[i].Nodes())
            {
                if (!byNode.TryGetValue(n, out var l))
                    byNode[n] = l = [];
                l.Add(i);
            }
        foreach (var l in byNode.Values)
            for (var i = 0; i < l.Count; i++)
            for (var j = i + 1; j < l.Count; j++)
                _adj.Add(N(l[i], l[j]));
    }

    public bool AreAdjacent(int a, int b) => _adj.Contains(N(a, b));

    private static (int, int) N(int a, int b) => a < b ? (a, b) : (b, a);
}
