using ImpactLab.Core.Contact;
using ImpactLab.Core.Fracture;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class CoarseFragmentContactTests
{
    private static readonly ContactSettings Contact = new()
    {
        NormalStiffnessNPerM = 1_000,
        NormalDampingNsPerM = 0,
        NodeRadiusScale = 0.5,
    };

    private static readonly DeformableContactRule Rule = new(
        "fragment",
        "target",
        StiffnessScale: 1,
        DampingScale: 1
    );

    [Fact]
    public void ResolvedContactTransfersEqualAndOppositeMomentum()
    {
        var material = MaterialLibrary.Get("steel-s355jr");
        var node = new MeshNode(0, "target", 0, 0, 0, Vec3.Zero, 1, 1, material);
        var mesh = new SimulationMesh(
            [node],
            [],
            1,
            new() { [("target", 0, 0, 0)] = 0 },
            [new("target", 0, 1, 0, 0, new(Vec3.Zero, Vec3.Zero))]
        );
        var resolved = new SimulationState(mesh);
        var fragment = new FragmentCoarseBodyState(
            Projection("fragment", new Vec3(0.75, 0, 0), -Vec3.UnitX)
        );
        var initialMomentum = resolved.Velocity[0] * node.MassKg + fragment.LinearMomentumKgMPerSec;
        var solver = new CoarseFragmentContactSolver();

        solver.BeginStep();
        var metrics = solver.ApplyResolved(
            mesh,
            resolved,
            fragment,
            [0],
            "target",
            Contact,
            Rule,
            1e-4
        );
        solver.EndStep();
        resolved.Velocity[0] += resolved.Force[0] / node.MassKg * 1e-4;
        fragment.Integrate(1e-4, Vec3.Zero);
        var finalMomentum = resolved.Velocity[0] * node.MassKg + fragment.LinearMomentumKgMPerSec;

        Assert.Equal(1, metrics.ContactCount);
        Assert.Equal(0, metrics.MomentumBalanceResidualN, 12);
        Assert.Equal(initialMomentum.X, finalMomentum.X, 12);
        Assert.Equal(initialMomentum.Y, finalMomentum.Y, 12);
        Assert.Equal(initialMomentum.Z, finalMomentum.Z, 12);
        Assert.True(metrics.CriticalTimeStepSeconds > 1e-4);
    }

    [Fact]
    public void SlidingResolvedContactDissipatesTangentialEnergyWithoutLosingMomentum()
    {
        var material = MaterialLibrary.Get("steel-s355jr");
        var node = new MeshNode(0, "target", 0, 0, 0, Vec3.Zero, 1, 1, material);
        var mesh = new SimulationMesh(
            [node],
            [],
            1,
            new() { [("target", 0, 0, 0)] = 0 },
            [new("target", 0, 1, 0, 0, new(Vec3.Zero, Vec3.Zero))]
        );
        var resolved = new SimulationState(mesh);
        var fragment = new FragmentCoarseBodyState(
            Projection("sliding", new Vec3(0, 0, 0.75), new Vec3(1, 0, -1))
        );
        var initialMomentum = fragment.LinearMomentumKgMPerSec;
        var solver = new CoarseFragmentContactSolver();

        solver.BeginStep();
        var metrics = solver.ApplyResolved(
            mesh,
            resolved,
            fragment,
            [0],
            "target",
            Contact,
            Rule,
            1e-4
        );
        solver.EndStep();
        resolved.Velocity[0] += resolved.Force[0] / node.MassKg * 1e-4;
        fragment.Integrate(1e-4, Vec3.Zero);
        var finalMomentum = resolved.Velocity[0] * node.MassKg + fragment.LinearMomentumKgMPerSec;

        Assert.True(metrics.TotalTangentialForceN > 0);
        Assert.True(metrics.DissipatedFrictionEnergyJ > 0);
        Assert.True(fragment.LinearVelocity.X < 1);
        Assert.True(resolved.Velocity[0].X > 0);
        Assert.Equal(initialMomentum.X, finalMomentum.X, 12);
        Assert.Equal(initialMomentum.Y, finalMomentum.Y, 12);
        Assert.Equal(initialMomentum.Z, finalMomentum.Z, 12);
    }

    [Fact]
    public void StaticContactRetainsTangentialDisplacementUntilContactEnds()
    {
        var material = MaterialLibrary.Get("steel-s355jr");
        var node = new MeshNode(0, "target", 0, 0, 0, Vec3.Zero, 1, 1, material);
        var mesh = new SimulationMesh(
            [node],
            [],
            1,
            new() { [("target", 0, 0, 0)] = 0 },
            [new("target", 0, 1, 0, 0, new(Vec3.Zero, Vec3.Zero))]
        );
        var resolved = new SimulationState(mesh);
        var fragment = new FragmentCoarseBodyState(
            Projection("sticking", new Vec3(0, 0, 0.75), Vec3.UnitX * 0.1)
        );
        var settings = Contact with
        {
            TangentialStiffnessNPerM = 10,
            TangentialDampingNsPerM = 0,
            StaticFrictionCoefficient = 10,
            DynamicFrictionCoefficient = 1,
        };
        var solver = new CoarseFragmentContactSolver();

        solver.BeginStep();
        var first = solver.ApplyResolved(
            mesh,
            resolved,
            fragment,
            [0],
            "target",
            settings,
            Rule,
            0.1
        );
        solver.EndStep();
        solver.BeginStep();
        var second = solver.ApplyResolved(
            mesh,
            resolved,
            fragment,
            [0],
            "target",
            settings,
            Rule,
            0.1
        );
        solver.EndStep();

        Assert.Equal(1, solver.HistoryCount);
        Assert.Equal(first.TotalTangentialForceN * 2, second.TotalTangentialForceN, 12);
        Assert.Equal(
            first.StoredTangentialPenaltyEnergyJ * 4,
            second.StoredTangentialPenaltyEnergyJ,
            12
        );
        Assert.Equal(0, second.DissipatedFrictionEnergyJ, 12);

        solver.BeginStep();
        solver.EndStep();
        Assert.Equal(0, solver.HistoryCount);
    }

    [Fact]
    public void UndampedHeadOnCoarseCollisionConservesMomentumAndEnergyWithinTolerance()
    {
        var left = new FragmentCoarseBodyState(
            Projection("left", new Vec3(-0.6, 0, 0), Vec3.UnitX)
        );
        var right = new FragmentCoarseBodyState(
            Projection("right", new Vec3(0.6, 0, 0), -Vec3.UnitX)
        );
        var initialMomentum = left.LinearMomentumKgMPerSec + right.LinearMomentumKgMPerSec;
        var initialEnergy = left.AccountedKineticEnergyJ + right.AccountedKineticEnergyJ;
        const double dt = 1e-4;
        var peakContactCount = 0;
        var minimumCriticalStep = double.PositiveInfinity;
        var solver = new CoarseFragmentContactSolver();

        for (var step = 0; step < 4_000; step++)
        {
            solver.BeginStep();
            var metrics = solver.ApplyPair(left, right, Contact, Rule, dt);
            solver.EndStep();
            peakContactCount = Math.Max(peakContactCount, metrics.ContactCount);
            minimumCriticalStep = Math.Min(minimumCriticalStep, metrics.CriticalTimeStepSeconds);
            left.Integrate(dt, Vec3.Zero);
            right.Integrate(dt, Vec3.Zero);
        }

        var finalMomentum = left.LinearMomentumKgMPerSec + right.LinearMomentumKgMPerSec;
        var finalEnergy = left.AccountedKineticEnergyJ + right.AccountedKineticEnergyJ;

        Assert.True(peakContactCount > 0);
        Assert.True(dt < minimumCriticalStep);
        Assert.Equal(initialMomentum.X, finalMomentum.X, 10);
        Assert.Equal(initialMomentum.Y, finalMomentum.Y, 10);
        Assert.Equal(initialMomentum.Z, finalMomentum.Z, 10);
        Assert.InRange(Math.Abs(finalEnergy - initialEnergy) / initialEnergy, 0, 0.01);
    }

    private static FragmentCoarseState Projection(string id, Vec3 center, Vec3 velocity) =>
        new(
            id,
            "fragment",
            FragmentRepresentationKind.ReducedOrderRigid,
            [0],
            [new(Vec3.Zero, 1)],
            1,
            center,
            velocity,
            Matrix3.Diagonal(1, 1, 1),
            1,
            Vec3.Zero,
            Vec3.Zero,
            QuaternionD.Identity,
            new(center - Vec3.UnitX * 0.5, center + Vec3.UnitX * 0.5),
            0.5 * velocity.LengthSquared,
            0,
            0
        );
}
