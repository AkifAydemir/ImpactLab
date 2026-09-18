using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.Core.Continuum.Dynamics;

public static class ContinuumDynamicLoadEvaluator
{
    public static double[] Evaluate(TetrahedralMesh mesh, ScenarioDefinition scenario, double time)
    {
        var f = new double[mesh.Nodes.Count * 3];
        for (var i = 0; i < mesh.Nodes.Count; i++)
        {
            var g = scenario.Settings.Gravity * mesh.Nodes[i].Material.DensityKgPerM3;
            f[i * 3] += g.X;
            f[i * 3 + 1] += g.Y;
            f[i * 3 + 2] += g.Z;
        }
        foreach (var l in scenario.Loads.Where(x => x.Enabled))
        {
            var active = l.DurationSeconds <= 0 || time <= l.DurationSeconds;
            if (!active)
                continue;
            var q = mesh
                .Nodes.Where(n =>
                    l.PartId is null
                    || n.PartId.Equals(l.PartId, StringComparison.OrdinalIgnoreCase)
                )
                .Where(n =>
                    Math.Abs(n.Position.X - l.Center.X) <= l.Size.X / 2
                    && Math.Abs(n.Position.Y - l.Center.Y) <= l.Size.Y / 2
                    && Math.Abs(n.Position.Z - l.Center.Z) <= l.Size.Z / 2
                )
                .ToArray();
            if (q.Length == 0)
                continue;
            var dir = l.Direction.LengthSquared < 1e-18 ? Vec3.UnitZ : l.Direction.Normalized();
            var envelope =
                l.DurationSeconds <= 0
                    ? 1.0
                    : Math.Sin(Math.PI * Math.Clamp(time / l.DurationSeconds, 0, 1));
            var each = dir * (l.Magnitude * envelope / q.Length);
            foreach (var n in q)
            {
                f[n.Id * 3] += each.X;
                f[n.Id * 3 + 1] += each.Y;
                f[n.Id * 3 + 2] += each.Z;
            }
        }
        return f;
    }
}
