using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Loads;

public sealed class CurveDirectionalLoad : ILoadSource
{
    public CurveDirectionalLoad(
        string? partId,
        Vec3 direction,
        PiecewiseLinearCurve forceCurve,
        Vec3 center,
        double radiusMeters
    )
    {
        PartId = partId;
        Direction = direction.Normalized();
        ForceCurve = forceCurve;
        Center = center;
        RadiusMeters = radiusMeters;
    }

    public string? PartId { get; }
    public Vec3 Direction { get; }
    public PiecewiseLinearCurve ForceCurve { get; }
    public Vec3 Center { get; }
    public double RadiusMeters { get; }

    public void Apply(
        double timeSeconds,
        double timeStepSeconds,
        SimulationMesh mesh,
        SimulationState state
    )
    {
        var magnitude = ForceCurve.Evaluate(timeSeconds);
        if (Math.Abs(magnitude) <= 1e-12)
            return;
        var selected = new List<int>();
        for (var i = 0; i < mesh.Nodes.Count; i++)
        {
            if (
                PartId is not null
                && !string.Equals(mesh.Nodes[i].PartId, PartId, StringComparison.OrdinalIgnoreCase)
            )
                continue;
            if ((state.Position[i] - Center).Length <= RadiusMeters)
                selected.Add(i);
        }
        if (selected.Count == 0)
            return;
        var each = Direction * (magnitude / selected.Count);
        foreach (var id in selected)
            state.Force[id] += each;
    }
}
