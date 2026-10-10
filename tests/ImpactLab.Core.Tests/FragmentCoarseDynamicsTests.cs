using ImpactLab.Core.Fracture;
using ImpactLab.Core.Mathematics;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class FragmentCoarseDynamicsTests
{
    [Fact]
    public void GravityAdvancesCenterOfMassAndPreservesSpin()
    {
        var state = new FragmentCoarseBodyState(CreateProjection("fragment-a", spinning: true));
        var initialAngularMomentum = state.AngularMomentumKgM2PerSec;
        var initialRotationalEnergy = state.RotationalKineticEnergyJ;

        state.Integrate(0.5, new Vec3(0, 0, -10));

        Assert.Equal(new Vec3(0.5, 0, -1.25), state.CenterOfMass);
        Assert.Equal(new Vec3(1, 0, -5), state.LinearVelocity);
        Assert.Equal(initialAngularMomentum.X, state.AngularMomentumKgM2PerSec.X, 12);
        Assert.Equal(initialAngularMomentum.Y, state.AngularMomentumKgM2PerSec.Y, 12);
        Assert.Equal(initialAngularMomentum.Z, state.AngularMomentumKgM2PerSec.Z, 12);
        Assert.Equal(initialRotationalEnergy, state.RotationalKineticEnergyJ, 12);
        Assert.Equal(Vec3.Zero, state.Force);
        Assert.Equal(Vec3.Zero, state.Torque);
    }

    [Fact]
    public void OffCenterForceProducesTranslationAndTorque()
    {
        var state = new FragmentCoarseBodyState(CreateProjection("fragment-b", spinning: false));
        state.ApplyForceAtPoint(Vec3.UnitX * 10, Vec3.UnitY);

        state.Integrate(0.1, Vec3.Zero);

        Assert.Equal(1.5, state.LinearVelocity.X, 12);
        Assert.Equal(-0.5, state.AngularVelocityRadPerSec.Z, 12);
        Assert.Equal(Vec3.Zero, state.Force);
        Assert.Equal(Vec3.Zero, state.Torque);
    }

    [Fact]
    public void LifecycleSpawnsEachStableFragmentIdentityOnce()
    {
        var tracker = new FragmentLifecycleTracker();
        var projection = CreateProjection("fragment-c", spinning: false);

        Assert.Single(tracker.SpawnNew([projection]));
        Assert.Empty(tracker.SpawnNew([projection]));
        Assert.Single(tracker.States);
        Assert.True(tracker.TryGet("fragment-c", out var state));
        Assert.NotNull(state);

        tracker.IntegrateAll(0.25, new Vec3(0, 0, -4));

        Assert.Equal(-1, state!.LinearVelocity.Z, 12);
    }

    [Fact]
    public void SingularFragmentInertiaIsRejectedBeforeIntegration()
    {
        var projection = CreateProjection("singular", spinning: false) with
        {
            InertiaTensorKgM2 = Matrix3.Diagonal(1, 1, 0),
            InertiaConditionNumber = double.PositiveInfinity,
        };

        var error = Assert.Throws<ArgumentException>(() => new FragmentCoarseBodyState(projection));

        Assert.Contains("singular or ill-conditioned", error.Message);
    }

    private static FragmentCoarseState CreateProjection(string id, bool spinning)
    {
        var angularVelocity = spinning ? Vec3.UnitZ * 2 : Vec3.Zero;
        return new(
            id,
            "part",
            FragmentRepresentationKind.ReducedOrderRigid,
            [0, 1],
            [new(Vec3.Zero, 1)],
            2,
            Vec3.Zero,
            Vec3.UnitX,
            Matrix3.Diagonal(2, 2, 2),
            1,
            Matrix3.Diagonal(2, 2, 2) * angularVelocity,
            angularVelocity,
            QuaternionD.Identity,
            new(-Vec3.UnitX, Vec3.UnitX),
            1,
            spinning ? 4 : 0,
            0.5
        );
    }
}
