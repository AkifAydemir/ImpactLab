using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Constraints;

public sealed class FixedPartConstraint : IFixedConstraint
{
    public FixedPartConstraint(string partId)
    {
        if (string.IsNullOrWhiteSpace(partId))
            throw new ArgumentException("Part id cannot be empty.", nameof(partId));
        PartId = partId;
    }

    public string PartId { get; }

    public void Apply(SimulationMesh mesh, SimulationState state)
    {
        var part = mesh.GetPart(PartId);
        for (var i = part.NodeStart; i < part.NodeEndExclusive; i++)
            state.IsFixed[i] = true;
    }
}
