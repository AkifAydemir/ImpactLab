using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.PostProcessing;

public static class FrameExtremaAnalyzer
{
    public static IReadOnlyList<FrameExtrema> Analyze(
        SimulationMesh mesh,
        SimulationResult result,
        ResultFieldKind field,
        string? partId = null
    )
    {
        var output = new List<FrameExtrema>(result.Frames.Count);
        for (var f = 0; f < result.Frames.Count; f++)
        {
            var rows = ResultQueryEngine.Execute(
                mesh,
                result,
                new ResultQuery(f, field, partId, Limit: int.MaxValue)
            );
            output.Add(
                rows.Count == 0
                    ? new FrameExtrema(f, result.Frames[f].TimeSeconds, 0, 0, 0, 0)
                    : new FrameExtrema(
                        f,
                        result.Frames[f].TimeSeconds,
                        rows.Min(x => x.Value),
                        rows.Max(x => x.Value),
                        rows.Average(x => x.Value),
                        rows.Count
                    )
            );
        }
        return output;
    }
}
