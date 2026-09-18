using ImpactLab.Core.Continuum.Results;

namespace ImpactLab.Core.PostProcessing;

public static class ContinuumProbeEvaluator
{
    public static IReadOnlyList<ProbeSeries> Evaluate(
        ContinuumResult result,
        IReadOnlyList<ProbeDefinition> probes
    )
    {
        var series = new List<ProbeSeries>();
        foreach (var probe in probes)
        {
            var nodeIds = probe
                .Selection.NodeIds.Where(id => id >= 0 && id < result.Mesh.Nodes.Count)
                .ToArray();
            var samples = new List<ProbeSample>();

            foreach (var frame in result.Frames)
            {
                var values = nodeIds.Select(id => ValueAt(probe.Quantity, frame, id)).ToArray();
                if (values.Length == 0)
                {
                    samples.Add(new(frame.TimeSeconds, 0, 0, 0, 0, 0));
                    continue;
                }

                var average = values.Average();
                var rms = Math.Sqrt(values.Average(value => value * value));
                samples.Add(
                    new(frame.TimeSeconds, values.Min(), values.Max(), average, rms, values.Length)
                );
            }

            series.Add(new(probe, samples));
        }

        return series;
    }

    private static double ValueAt(ProbeQuantity quantity, ContinuumFrame frame, int nodeId) =>
        quantity switch
        {
            ProbeQuantity.DisplacementMagnitude => frame.Displacement[nodeId].Length,
            ProbeQuantity.PositionX => frame.Position[nodeId].X,
            ProbeQuantity.PositionY => frame.Position[nodeId].Y,
            ProbeQuantity.PositionZ => frame.Position[nodeId].Z,
            ProbeQuantity.Speed => 0,
            ProbeQuantity.Damage => 0,
            _ => 0,
        };
}
