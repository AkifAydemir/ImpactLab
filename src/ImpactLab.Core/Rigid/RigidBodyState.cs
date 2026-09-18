using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Rigid;

public sealed class RigidBodyState
{
    public RigidBodyState(RigidBodyDefinition definition)
    {
        Definition = definition;
        Velocity = definition.InitialVelocity;
        AngularVelocityRadPerSec = definition.InitialAngularVelocityRadPerSec;
        Orientation = definition.InitialOrientation;
    }

    public RigidBodyDefinition Definition { get; }
    public double MassKg => Definition.MassKg;
    public Vec3 PositionOffset { get; set; }
    public Vec3 Velocity { get; set; }
    public Vec3 Force { get; set; }
    public Vec3 Torque { get; set; }
    public QuaternionD Orientation { get; set; }
    public Vec3 AngularVelocityRadPerSec { get; set; }
    public Vec3 WorldCenter => Definition.Geometry.Center + PositionOffset;
    public RigidTransform Transform => new(WorldCenter, Orientation);

    public Vec3 VelocityAtWorldPoint(in Vec3 point)
    {
        var radius = point - WorldCenter;
        return Velocity + Vec3.Cross(AngularVelocityRadPerSec, radius);
    }

    public void ApplyForceAtPoint(in Vec3 force, in Vec3 worldPoint)
    {
        Force += force;
        Torque += Vec3.Cross(worldPoint - WorldCenter, force);
    }

    public void Integrate(double timeStepSeconds, double damping)
    {
        if (Definition.IsKinematic)
        {
            Force = Vec3.Zero;
            Torque = Vec3.Zero;
            return;
        }

        var dampingFactor = Math.Clamp(1.0 - damping, 0.0, 1.0);
        Velocity = (Velocity + Force / Math.Max(MassKg, 1e-12) * timeStepSeconds) * dampingFactor;
        PositionOffset += Velocity * timeStepSeconds;
        var localTorque = Orientation.InverseRotate(Torque);
        var localAcceleration = Definition.InertiaBodyKgM2.InverseDiagonal() * localTorque;
        var worldAcceleration = Orientation.Rotate(localAcceleration);
        AngularVelocityRadPerSec =
            (AngularVelocityRadPerSec + worldAcceleration * timeStepSeconds) * dampingFactor;
        Orientation = (
            QuaternionD.FromAngularVelocity(AngularVelocityRadPerSec, timeStepSeconds) * Orientation
        ).Normalized();
        Force = Vec3.Zero;
        Torque = Vec3.Zero;
    }
}
