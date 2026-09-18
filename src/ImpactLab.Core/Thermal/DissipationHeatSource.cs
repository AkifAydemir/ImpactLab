namespace ImpactLab.Core.Thermal;

public static class DissipationHeatSource
{
    public static double[] FromElementDissipation(
        ImpactLab.Core.Continuum.TetrahedralMesh mesh,
        ReadOnlySpan<double> dissipatedJ,
        double fraction,
        double dt
    )
    {
        var q = new double[mesh.Nodes.Count];
        for (var e = 0; e < mesh.Elements.Count; e++)
        {
            var power =
                Math.Max(0, dissipatedJ[e]) * Math.Clamp(fraction, 0, 1) / Math.Max(dt, 1e-30) / 4;
            foreach (var n in mesh.Elements[e].Nodes())
                q[n] += power;
        }
        return q;
    }
}
