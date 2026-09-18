using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Selection;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Loads;

public sealed class ImpulseLoad : ILoadSource
{
    public ImpulseLoad(
        NodeSelection selection,
        Vec3 direction,
        double totalImpulseNs,
        double durationSeconds
    )
    {
        if (durationSeconds <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds));
        Selection = selection;
        Direction = direction.Normalized();
        TotalImpulseNs = totalImpulseNs;
        DurationSeconds = durationSeconds;
    }

    public NodeSelection Selection { get; }
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
        if (timeSeconds < 0.0 || timeSeconds > DurationSeconds || Selection.IsEmpty)
            return;
        var forcePerNode = TotalImpulseNs / DurationSeconds / Selection.Count;
        foreach (var id in Selection.NodeIds)
            state.Force[id] += Direction * forcePerNode;
    }
}
