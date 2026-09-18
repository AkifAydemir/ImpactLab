using ImpactLab.Core.Continuum.Results;

namespace ImpactLab.Core.Analysis;

public static class ContinuumResultAnalyzer
{
    public static ContinuumResultSummary Analyze(ContinuumResult r)
    {
        var last = r.Frames.Last();
        return new(
            r.Mesh.Nodes.Count,
            r.Mesh.Elements.Count,
            last.Displacement.Max(x => x.Length),
            last.Elements.VonMisesPa.DefaultIfEmpty().Max(),
            last.TemperatureKelvin.DefaultIfEmpty(293.15).Max(),
            last.TemperatureKelvin.DefaultIfEmpty(293.15).Min(),
            r.LinearSolves.All(x => x.Converged),
            r.LinearSolves.Sum(x => x.Iterations)
        );
    }
}
