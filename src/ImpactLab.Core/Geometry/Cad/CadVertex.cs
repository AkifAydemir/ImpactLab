using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry.Cad;

public sealed record CadVertex(CadEntityId Id, Vec3 Position, double ToleranceMeters);
