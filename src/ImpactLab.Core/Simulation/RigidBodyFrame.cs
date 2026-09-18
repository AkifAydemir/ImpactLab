using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Simulation;

public sealed record RigidBodyFrame(
    string Id,
    Vec3 PositionOffset,
    Vec3 Velocity,
    QuaternionD Orientation,
    Vec3 AngularVelocityRadPerSec
);
