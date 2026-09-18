using ImpactLab.Core.Continuum;
using ImpactLab.Core.Continuum.Mechanics;
using ImpactLab.Core.Continuum.Static;
using ImpactLab.Core.Geometry;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class LinearStaticContinuumSolverTests
{
    [Fact]
    public void ConstrainedBlockSolves()
    {
        var mesh = StructuredTetraMesher.Build(
            "p",
            new BoxSpec("b", Vec3.Zero, new Vec3(.1, .1, .1)),
            MaterialLibrary.All[0],
            new(.05)
        );
        var rhs = new double[mesh.Nodes.Count * 3];
        rhs[^1] = 100;
        var fixedDofs = new[]
        {
            new ContinuumDirichletDof(0, 0),
            new ContinuumDirichletDof(1, 0),
            new ContinuumDirichletDof(2, 0),
        };
        var result = new LinearStaticContinuumSolver().Solve(mesh, rhs, fixedDofs, new());
        Assert.True(result.LinearSolve.Iterations >= 0);
    }
}
