using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Loads;

public sealed class PressurePulseLoad : ILoadSource
{
    public PressurePulseLoad(
        Vec3 origin,
        double radius,
        double peakPressurePa,
        double durationSeconds
    )
    {
        if (radius <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(radius));
        if (peakPressurePa < 0.0)
            throw new ArgumentOutOfRangeException(nameof(peakPressurePa));
        if (durationSeconds <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds));
        Origin = origin;
        Radius = radius;
        PeakPressurePa = peakPressurePa;
        DurationSeconds = durationSeconds;
    }

    public Vec3 Origin { get; }
    public double Radius { get; }
    public double PeakPressurePa { get; }
    public double DurationSeconds { get; }

    public void Apply(
        double timeSeconds,
        double timeStepSeconds,
        SimulationMesh mesh,
        SimulationState state
    )
    {
        if (timeSeconds < 0.0 || timeSeconds > DurationSeconds)
            return;
        var phase = Math.Clamp(timeSeconds / DurationSeconds, 0.0, 1.0);
        var temporal = (1.0 - phase) * (1.0 - phase);
        var faceArea = mesh.CellSize * mesh.CellSize;
        for (var i = 0; i < mesh.Nodes.Count; i++)
        {
            var radial = state.Position[i] - Origin;
            var distance = radial.Length;
            if (distance <= 1e-12 || distance > Radius)
                continue;
            var spatial = 1.0 - distance / Radius;
            var pressure = PeakPressurePa * temporal * spatial * spatial;
            state.Force[i] += radial.Normalized() * (pressure * faceArea);
        }
    }
}
