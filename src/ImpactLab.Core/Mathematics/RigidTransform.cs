namespace ImpactLab.Core.Mathematics;

public readonly record struct RigidTransform(Vec3 Translation, QuaternionD Rotation)
{
    public static RigidTransform Identity { get; } = new(Vec3.Zero, QuaternionD.Identity);

    public Vec3 TransformPoint(in Vec3 local) => Translation + Rotation.Rotate(local);

    public Vec3 TransformVector(in Vec3 local) => Rotation.Rotate(local);

    public Vec3 InverseTransformPoint(in Vec3 world) => Rotation.InverseRotate(world - Translation);

    public Vec3 InverseTransformVector(in Vec3 world) => Rotation.InverseRotate(world);
}
