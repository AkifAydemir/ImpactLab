using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Loads;

public sealed class DirectionalPulseLoad : ILoadSource
{
    public DirectionalPulseLoad(
        Vec3 center,
        double radius,
        Vec3 direction,
        double totalImpulseNs,
        double durationSeconds
    )
    {
        if (radius <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(radius));
        if (durationSeconds <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds));
        if (totalImpulseNs < 0.0)
            throw new ArgumentOutOfRangeException(nameof(totalImpulseNs));
        Center = center;
        Radius = radius;
        Direction = direction.Normalized();
        TotalImpulseNs = totalImpulseNs;
        DurationSeconds = durationSeconds;
    }

    public Vec3 Center { get; }
    public double Radius { get; }
    public Vec3 Direction { get; }
    public double TotalImpulseNs { get; }
    public double DurationSeconds { get; }

    public void Apply(
        double timeSeconds,
        double timeStepSeconds,
        SimulationMesh mesh,
        SimulationState state
    )
    {
        if (timeSeconds < 0.0 || timeSeconds > DurationSeconds || Direction.LengthSquared <= 1e-12)
            return;
        var weights = new double[mesh.Nodes.Count];
        var weightSum = 0.0;
        for (var i = 0; i < mesh.Nodes.Count; i++)
        {
            var distance = (state.Position[i] - Center).Length;
            if (distance > Radius)
                continue;
            var normalized = distance / Radius;
            var weight = Math.Exp(-4.0 * normalized * normalized);
            weights[i] = weight;
            weightSum += weight;
        }
        if (weightSum <= 1e-12)
            return;
        var phase = Math.Clamp(timeSeconds / DurationSeconds, 0.0, 1.0);
        var envelope = Math.Sin(Math.PI * phase);
        var envelopeIntegral = 2.0 * DurationSeconds / Math.PI;
        var peakForce = TotalImpulseNs / envelopeIntegral;
        for (var i = 0; i < mesh.Nodes.Count; i++)
        {
            if (weights[i] <= 0.0)
                continue;
            var share = weights[i] / weightSum;
            state.Force[i] += Direction * (peakForce * envelope * share);
        }
    }
}
