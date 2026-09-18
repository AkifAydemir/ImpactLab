using ImpactLab.Core.Continuum.Mechanics;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.Core.Continuum;

public static class ContinuumBoundaryCompiler
{
    public static IReadOnlyList<ContinuumDirichletDof> Compile(
        TetrahedralMesh mesh,
        ScenarioDefinition scenario
    )
    {
        var result = new Dictionary<int, double>();
        foreach (var part in scenario.Parts.Where(x => x.FixMinimumZPlane))
        {
            var nodes = mesh.Nodes.Where(n =>
                n.PartId.Equals(part.Id, StringComparison.OrdinalIgnoreCase)
            );
            var min = nodes.Min(n => n.Position.Z);
            var tol = scenario.Continuum.TargetEdgeLengthMeters * 0.55;
            foreach (var n in nodes.Where(n => Math.Abs(n.Position.Z - min) <= tol))
            {
                result[n.Id * 3] = 0;
                result[n.Id * 3 + 1] = 0;
                result[n.Id * 3 + 2] = 0;
            }
        }
        foreach (var b in scenario.Boundaries.Where(x => x.Enabled))
        {
            var selected = mesh
                .Nodes.Where(n =>
                    b.PartId is null
                    || n.PartId.Equals(b.PartId, StringComparison.OrdinalIgnoreCase)
                )
                .Where(n =>
                    n.Position.X >= b.Center.X - b.Size.X / 2
                    && n.Position.X <= b.Center.X + b.Size.X / 2
                    && n.Position.Y >= b.Center.Y - b.Size.Y / 2
                    && n.Position.Y <= b.Center.Y + b.Size.Y / 2
                    && n.Position.Z >= b.Center.Z - b.Size.Z / 2
                    && n.Position.Z <= b.Center.Z + b.Size.Z / 2
                );
            foreach (var n in selected)
            {
                if (b.LockX)
                    result[n.Id * 3] = 0;
                if (b.LockY)
                    result[n.Id * 3 + 1] = 0;
                if (b.LockZ)
                    result[n.Id * 3 + 2] = 0;
            }
        }
        return result.Select(x => new ContinuumDirichletDof(x.Key, x.Value)).ToArray();
    }
}
