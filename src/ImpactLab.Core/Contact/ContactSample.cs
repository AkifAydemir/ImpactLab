using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Contact;

public readonly record struct ContactSample(double SignedDistanceMeters, Vec3 OutwardNormal);
