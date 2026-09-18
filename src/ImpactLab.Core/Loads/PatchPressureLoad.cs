using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Selection;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Loads;

public sealed class PatchPressureLoad : ILoadSource
{
    public PatchPressureLoad(
        NodeSelection selection,
        Vec3 outwardNormal,
        PiecewiseLinearCurve pressurePa
    )
    {
        Selection = selection;
        Normal = outwardNormal.Normalized();
        PressurePa = pressurePa;
        if (Normal.LengthSquared <= 1e-18)
            throw new ArgumentException(
                "Pressure direction cannot be zero.",
                nameof(outwardNormal)
            );
    }

    public NodeSelection Selection { get; }
    public Vec3 Normal { get; }
    public PiecewiseLinearCurve PressurePa { get; }

    public void Apply(
        double timeSeconds,
        double timeStepSeconds,
        SimulationMesh mesh,
        SimulationState state
    )
    {
        var pressure = PressurePa.Evaluate(timeSeconds);
        if (Math.Abs(pressure) <= 1e-18 || Selection.IsEmpty)
            return;
        foreach (var id in Selection.NodeIds)
        {
            var area = mesh.Nodes[id].CellSize * mesh.Nodes[id].CellSize;
            state.Force[id] += Normal * (pressure * area);
        }
    }
}
