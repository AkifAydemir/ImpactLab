using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Selection;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Loads;

public sealed class BodyAccelerationLoad : ILoadSource
{
    public BodyAccelerationLoad(
        NodeSelection selection,
        Vec3 accelerationMps2,
        PiecewiseLinearCurve? scale = null
    )
    {
        Selection = selection;
        AccelerationMps2 = accelerationMps2;
        Scale = scale;
    }

    public NodeSelection Selection { get; }
    public Vec3 AccelerationMps2 { get; }
    public PiecewiseLinearCurve? Scale { get; }

    public void Apply(
        double timeSeconds,
        double timeStepSeconds,
        SimulationMesh mesh,
        SimulationState state
    )
    {
        var factor = Scale?.Evaluate(timeSeconds) ?? 1.0;
        foreach (var id in Selection.NodeIds)
            state.Force[id] += AccelerationMps2 * (mesh.Nodes[id].MassKg * factor);
    }
}
