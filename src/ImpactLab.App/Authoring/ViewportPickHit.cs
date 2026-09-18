using ImpactLab.Core.Mathematics;

namespace ImpactLab.App.Authoring;

public sealed record ViewportPickHit(
    string ObjectId,
    double Distance,
    Vec3 WorldPoint,
    string Backend
);
