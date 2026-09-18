using ImpactLab.Core.Continuum.Mechanics;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.Core.Continuum;

public static class ContinuumLoadCompiler
{
    public static double[] Compile(TetrahedralMesh mesh, ScenarioDefinition scenario)
    {
        var rhs = ContinuumLoadVectorAssembler.BodyForce(mesh, scenario.Settings.Gravity);
        foreach (var l in scenario.Loads.Where(x => x.Enabled))
        {
            var selected = mesh
                .Nodes.Where(n =>
                    l.PartId is null
                    || n.PartId.Equals(l.PartId, StringComparison.OrdinalIgnoreCase)
                )
                .Where(n =>
                    n.Position.X >= l.Center.X - l.Size.X / 2
                    && n.Position.X <= l.Center.X + l.Size.X / 2
                    && n.Position.Y >= l.Center.Y - l.Size.Y / 2
                    && n.Position.Y <= l.Center.Y + l.Size.Y / 2
                    && n.Position.Z >= l.Center.Z - l.Size.Z / 2
                    && n.Position.Z <= l.Center.Z + l.Size.Z / 2
                )
                .ToArray();
            if (selected.Length == 0)
                continue;
            var direction =
                l.Direction.LengthSquared < 1e-18 ? Vec3.UnitZ : l.Direction.Normalized();
            var force = direction * (l.Magnitude / selected.Length);
            foreach (var n in selected)
                ContinuumLoadVectorAssembler.AddNodal(rhs, n.Id, force);
        }
        return rhs;
    }
}
