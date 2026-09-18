using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Constraints;

public enum ConstraintAxis
{
    X,
    Y,
    Z,
}

public enum ConstraintSide
{
    Minimum,
    Maximum,
}

public sealed class FixedPlaneConstraint : IFixedConstraint
{
    public FixedPlaneConstraint(
        ConstraintAxis axis,
        ConstraintSide side,
        double toleranceMeters,
        string? partId = null
    )
    {
        if (toleranceMeters < 0.0)
            throw new ArgumentOutOfRangeException(nameof(toleranceMeters));
        Axis = axis;
        Side = side;
        ToleranceMeters = toleranceMeters;
        PartId = partId;
    }

    public ConstraintAxis Axis { get; }
    public ConstraintSide Side { get; }
    public double ToleranceMeters { get; }
    public string? PartId { get; }

    public void Apply(SimulationMesh mesh, SimulationState state)
    {
        var indices = Enumerable
            .Range(0, mesh.Nodes.Count)
            .Where(i =>
                PartId is null
                || string.Equals(mesh.Nodes[i].PartId, PartId, StringComparison.Ordinal)
            )
            .ToArray();
        if (indices.Length == 0)
            throw new InvalidOperationException(
                PartId is null
                    ? "Constraint matched no mesh nodes."
                    : $"Constraint part not found: {PartId}"
            );
        var values = indices.Select(i => GetAxisValue(mesh.Nodes[i])).ToArray();
        var target = Side == ConstraintSide.Minimum ? values.Min() : values.Max();
        for (var n = 0; n < indices.Length; n++)
        {
            if (Math.Abs(values[n] - target) <= ToleranceMeters)
                state.IsFixed[indices[n]] = true;
        }
    }

    private double GetAxisValue(MeshNode node) =>
        Axis switch
        {
            ConstraintAxis.X => node.RestPosition.X,
            ConstraintAxis.Y => node.RestPosition.Y,
            ConstraintAxis.Z => node.RestPosition.Z,
            _ => throw new InvalidOperationException($"Unsupported constraint axis: {Axis}."),
        };
}
