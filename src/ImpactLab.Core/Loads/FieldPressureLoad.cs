using ImpactLab.Core.Fields;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Selection;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Loads;

public sealed class FieldPressureLoad : ILoadSource
{
    public FieldPressureLoad(
        NodeSelection selection,
        Vec3 direction,
        PiecewiseLinearCurve pressurePa,
        IScalarField3 spatialField
    )
    {
        Selection = selection;
        Direction = direction.Normalized();
        PressurePa = pressurePa;
        SpatialField = spatialField;
        if (Direction.LengthSquared <= 1e-18)
            throw new ArgumentException("Pressure direction cannot be zero.", nameof(direction));
    }

    public NodeSelection Selection { get; }
    public Vec3 Direction { get; }
    public PiecewiseLinearCurve PressurePa { get; }
    public IScalarField3 SpatialField { get; }

    public void Apply(
        double timeSeconds,
        double timeStepSeconds,
        SimulationMesh mesh,
        SimulationState state
    )
    {
        var temporal = PressurePa.Evaluate(timeSeconds);
        if (Math.Abs(temporal) <= 1e-18)
            return;
        foreach (var id in Selection.NodeIds)
        {
            var scale = SpatialField.Sample(state.Position[id], timeSeconds);
            if (Math.Abs(scale) <= 1e-18)
                continue;
            var area = mesh.Nodes[id].CellSize * mesh.Nodes[id].CellSize;
            state.Force[id] += Direction * (temporal * scale * area);
        }
    }
}
