using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.PostProcessing;

public static class ProbeEvaluator
{
    public static IReadOnlyList<ProbeSeries> Evaluate(
        SimulationMesh mesh,
        SimulationResult result,
        IReadOnlyList<ProbeDefinition> probes
    ) =>
        probes
            .Select(p => new ProbeSeries(
                p,
                result.Frames.Select(f => EvaluateFrame(mesh, f, p)).ToArray()
            ))
            .ToArray();

    private static ProbeSample EvaluateFrame(
        SimulationMesh mesh,
        SimulationFrame frame,
        ProbeDefinition probe
    )
    {
        if (probe.Selection.IsEmpty)
            return new(frame.TimeSeconds, 0, 0, 0, 0, 0);
        var values = new double[probe.Selection.Count];
        for (var n = 0; n < probe.Selection.Count; n++)
        {
            var i = probe.Selection.NodeIds[n];
            var node = mesh.Nodes[i];
            values[n] = probe.Quantity switch
            {
                ProbeQuantity.DisplacementMagnitude => (
                    frame.Position[i] - node.RestPosition
                ).Length,
                ProbeQuantity.Speed => frame.Velocity[i].Length,
                ProbeQuantity.Damage => frame.NodeDamage[i],
                ProbeQuantity.PositionX => frame.Position[i].X,
                ProbeQuantity.PositionY => frame.Position[i].Y,
                ProbeQuantity.PositionZ => frame.Position[i].Z,
                _ => 0.0,
            };
        }
        var avg = values.Average();
        var rms = Math.Sqrt(values.Select(x => x * x).Average());
        return new(frame.TimeSeconds, values.Min(), values.Max(), avg, rms, values.Length);
    }
}
