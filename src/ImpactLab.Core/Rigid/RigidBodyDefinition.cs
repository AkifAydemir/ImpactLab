using ImpactLab.Core.Geometry;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Rigid;

public sealed class RigidBodyDefinition
{
    public RigidBodyDefinition(
        string id,
        string name,
        GeometrySpec geometry,
        MaterialDefinition material,
        Vec3 initialVelocity,
        double? massOverrideKg = null,
        bool isKinematic = false,
        Vec3? initialAngularVelocityRadPerSec = null,
        QuaternionD? initialOrientation = null
    )
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Rigid body id cannot be empty.", nameof(id));
        material.Validate();
        Geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
        var calculatedMass = GeometryVolume.Calculate(geometry) * material.DensityKgPerM3;
        if (massOverrideKg is <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(massOverrideKg));
        Id = id;
        Name = name;
        Material = material;
        InitialVelocity = initialVelocity;
        InitialAngularVelocityRadPerSec = initialAngularVelocityRadPerSec ?? Vec3.Zero;
        InitialOrientation = (initialOrientation ?? QuaternionD.Identity).Normalized();
        MassKg = massOverrideKg ?? calculatedMass;
        InertiaBodyKgM2 = RigidBodyInertia.Approximate(geometry, MassKg);
        IsKinematic = isKinematic;
    }

    public string Id { get; }
    public string Name { get; }
    public GeometrySpec Geometry { get; }
    public MaterialDefinition Material { get; }
    public Vec3 InitialVelocity { get; }
    public Vec3 InitialAngularVelocityRadPerSec { get; }
    public QuaternionD InitialOrientation { get; }
    public double MassKg { get; }
    public Matrix3 InertiaBodyKgM2 { get; }
    public bool IsKinematic { get; }
}
