namespace ImpactLab.Core.Geometry.Cad;

public sealed class ManagedGeometryHealer : IGeometryHealer
{
    public GeometryHealingReport Heal(
        CadModel model,
        GeometryHealingSettings s,
        CancellationToken ct = default
    )
    {
        ct.ThrowIfCancellationRequested();
        var remaining = CadTopologyInspector.Inspect(
            model,
            s.RemoveEdgeBelowMeters,
            s.RemoveFaceBelowM2
        );
        return new(
            model,
            0,
            remaining.Count(x => x.Kind == CadTopologyIssueKind.ShortEdge),
            remaining.Count(x => x.Kind == CadTopologyIssueKind.SmallFace),
            0,
            remaining
        );
    }
}
