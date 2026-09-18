namespace ImpactLab.Core.Meshing;

public static class MeshQualityAnalyzer
{
    public static MeshQualityReport Analyze(SimulationMesh mesh)
    {
        var degree = new int[mesh.Nodes.Count];
        foreach (var s in mesh.Springs)
        {
            degree[s.NodeA]++;
            degree[s.NodeB]++;
        }
        var surface = SurfaceNodeExtractor.Build(mesh).ByPart.Values.Sum(x => x.Length);
        var masses = mesh.Nodes.Select(x => x.MassKg).ToArray();
        var lengths = mesh.Springs.Select(x => x.RestLength).ToArray();
        return new(
            mesh.Nodes.Count,
            mesh.Springs.Count,
            mesh.Parts.Count,
            surface,
            degree.Count(x => x == 0),
            degree.Length == 0 ? 0 : degree.Min(),
            degree.Length == 0 ? 0 : degree.Max(),
            degree.Length == 0 ? 0 : degree.Average(),
            masses.Length == 0 ? 0 : masses.Min(),
            masses.Length == 0 ? 0 : masses.Max(),
            lengths.Length == 0 ? 0 : lengths.Min(),
            lengths.Length == 0 ? 0 : lengths.Max(),
            mesh.Nodes.Count == 0 ? 0.0 : (double)surface / mesh.Nodes.Count
        );
    }
}
