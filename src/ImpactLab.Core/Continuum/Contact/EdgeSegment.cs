using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Contact;

public readonly record struct EdgeSegment(int Id, int A, int B, Vec3 P0, Vec3 P1);
