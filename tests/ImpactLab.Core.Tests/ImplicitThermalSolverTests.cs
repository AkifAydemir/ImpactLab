using ImpactLab.Core.Continuum;
using ImpactLab.Core.Geometry;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Thermal;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class ImplicitThermalSolverTests
{
    [Fact]
    public void FixedHotFaceRaisesTemperature()
    {
        var mesh = StructuredTetraMesher.Build(
            "p",
            new BoxSpec("b", Vec3.Zero, new Vec3(.1, .1, .1)),
            MaterialLibrary.All[0],
            new(.05)
        );
        var state = new ThermalState(mesh.Nodes.Count);
        var ids = mesh
            .Nodes.Where(n => n.Position.Z == mesh.Nodes.Max(x => x.Position.Z))
            .Select(n => n.Id)
            .ToArray();
        new ImplicitThermalSolver().Step(
            mesh,
            state,
            new ThermalPropertyTable(),
            [new(ids, 400)],
            [],
            .1
        );
        Assert.True(ids.All(i => state.TemperatureKelvin[i] > 399));
    }
}
