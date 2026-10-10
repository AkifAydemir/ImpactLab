using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Fracture;

public sealed class FragmentCoarseBodyState
{
    private readonly Matrix3 _inverseInertiaBodyKgM2;

    public FragmentCoarseBodyState(FragmentCoarseState source)
    {
        if (source.Representation == FragmentRepresentationKind.ResolvedDeformable)
            throw new ArgumentException(
                "A resolved fragment cannot be integrated as a coarse body.",
                nameof(source)
            );
        if (source.MassKg <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(source),
                "Fragment mass must be positive."
            );
        if (
            !source.InertiaTensorKgM2.TryInverse(
                out _inverseInertiaBodyKgM2,
                out var conditionNumber
            )
        )
            throw new ArgumentException(
                $"Fragment '{source.Id}' has a singular or ill-conditioned inertia tensor.",
                nameof(source)
            );
        if (
            !double.IsFinite(source.InertiaConditionNumber)
            || Math.Abs(conditionNumber - source.InertiaConditionNumber)
                > 1e-6 * Math.Max(conditionNumber, 1.0)
        )
            throw new ArgumentException(
                $"Fragment '{source.Id}' inertia diagnostics do not match its tensor.",
                nameof(source)
            );
        Source = source;
        CenterOfMass = source.CenterOfMass;
        LinearVelocity = source.LinearVelocity;
        Orientation = source.Orientation;
        AngularVelocityRadPerSec = source.AngularVelocityRadPerSec;
    }

    public FragmentCoarseState Source { get; }
    public string Id => Source.Id;
    public string PartId => Source.PartId;
    public FragmentRepresentationKind Representation => Source.Representation;
    public double MassKg => Source.MassKg;
    public Matrix3 InertiaBodyKgM2 => Source.InertiaTensorKgM2;
    public Vec3 CenterOfMass { get; private set; }
    public Vec3 LinearVelocity { get; private set; }
    public QuaternionD Orientation { get; private set; }
    public Vec3 AngularVelocityRadPerSec { get; private set; }
    public Vec3 Force { get; private set; }
    public Vec3 Torque { get; private set; }
    public double UnresolvedInternalKineticEnergyJ => Source.UnresolvedInternalKineticEnergyJ;
    public Vec3 LinearMomentumKgMPerSec => LinearVelocity * MassKg;

    public Vec3 VelocityAtWorldPoint(in Vec3 point) =>
        LinearVelocity + Vec3.Cross(AngularVelocityRadPerSec, point - CenterOfMass);

    public Vec3 ContactPointWorld(FragmentContactSample sample) =>
        CenterOfMass + Orientation.Rotate(sample.LocalPosition);

    public Vec3 AngularMomentumKgM2PerSec
    {
        get
        {
            var omegaBody = Orientation.InverseRotate(AngularVelocityRadPerSec);
            return Orientation.Rotate(InertiaBodyKgM2 * omegaBody);
        }
    }

    public double TranslationalKineticEnergyJ => 0.5 * MassKg * LinearVelocity.LengthSquared;

    public double RotationalKineticEnergyJ
    {
        get
        {
            var omegaBody = Orientation.InverseRotate(AngularVelocityRadPerSec);
            return 0.5 * Vec3.Dot(omegaBody, InertiaBodyKgM2 * omegaBody);
        }
    }

    public double AccountedKineticEnergyJ =>
        TranslationalKineticEnergyJ + RotationalKineticEnergyJ + UnresolvedInternalKineticEnergyJ;

    public void ApplyForce(in Vec3 force)
    {
        Force += force;
    }

    public void ApplyTorque(in Vec3 torque)
    {
        Torque += torque;
    }

    public void ApplyForceAtPoint(in Vec3 force, in Vec3 worldPoint)
    {
        Force += force;
        Torque += Vec3.Cross(worldPoint - CenterOfMass, force);
    }

    public void Integrate(
        double timeStepSeconds,
        in Vec3 gravity,
        double linearDamping = 0.0,
        double angularDamping = 0.0
    )
    {
        if (timeStepSeconds <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(timeStepSeconds),
                "Time step must be positive."
            );
        if (linearDamping is < 0 or >= 1)
            throw new ArgumentOutOfRangeException(
                nameof(linearDamping),
                "Linear damping must be in [0, 1)."
            );
        if (angularDamping is < 0 or >= 1)
            throw new ArgumentOutOfRangeException(
                nameof(angularDamping),
                "Angular damping must be in [0, 1)."
            );

        var acceleration = gravity + Force / MassKg;
        CenterOfMass +=
            LinearVelocity * timeStepSeconds
            + acceleration * (0.5 * timeStepSeconds * timeStepSeconds);
        LinearVelocity = (LinearVelocity + acceleration * timeStepSeconds) * (1.0 - linearDamping);

        var omegaBody = Orientation.InverseRotate(AngularVelocityRadPerSec);
        var torqueBody = Orientation.InverseRotate(Torque);
        var angularMomentumBody = InertiaBodyKgM2 * omegaBody;
        var angularAccelerationBody =
            _inverseInertiaBodyKgM2 * (torqueBody - Vec3.Cross(omegaBody, angularMomentumBody));
        var omegaMidBody = omegaBody + angularAccelerationBody * (0.5 * timeStepSeconds);
        var omegaWorldMid = Orientation.Rotate(omegaMidBody);
        Orientation = (
            QuaternionD.FromAngularVelocity(omegaWorldMid, timeStepSeconds) * Orientation
        ).Normalized();
        var omegaBodyNext =
            (omegaBody + angularAccelerationBody * timeStepSeconds) * (1.0 - angularDamping);
        AngularVelocityRadPerSec = Orientation.Rotate(omegaBodyNext);

        Force = Vec3.Zero;
        Torque = Vec3.Zero;
    }

    public FragmentCoarseState Snapshot()
    {
        var translation = CenterOfMass - Source.CenterOfMass;
        return new(
            Id,
            PartId,
            Representation,
            Source.SourceNodeIds,
            Source.ContactSamples,
            MassKg,
            CenterOfMass,
            LinearVelocity,
            InertiaBodyKgM2,
            Source.InertiaConditionNumber,
            AngularMomentumKgM2PerSec,
            AngularVelocityRadPerSec,
            Orientation,
            new(Source.Bounds.Min + translation, Source.Bounds.Max + translation),
            TranslationalKineticEnergyJ,
            RotationalKineticEnergyJ,
            UnresolvedInternalKineticEnergyJ
        );
    }
}
