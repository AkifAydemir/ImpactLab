using ImpactLab.Core.Contact;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Rigid;

namespace ImpactLab.Core.Continuum.Contact;

public sealed record SurfaceContactForce(
    int TriangleId,
    Vec3 Force,
    Vec3 Point,
    double Penetration
);

public sealed class RigidSurfaceTriangleContactSolver
{
    public IReadOnlyList<SurfaceContactForce> Evaluate(
        SurfaceMesh surface,
        RigidBodyState body,
        SurfaceContactSettings settings
    )
    {
        var result = new List<SurfaceContactForce>();
        for (var i = 0; i < surface.Triangles.Count; i++)
        {
            var t = surface.Triangles[i];
            var pa = surface.Volume.Nodes[t.A].Position;
            var pb = surface.Volume.Nodes[t.B].Position;
            var pc = surface.Volume.Nodes[t.C].Position;
            var centroid = (pa + pb + pc) / 3;
            var sample = GeometryContact.Sample(body.Definition.Geometry, body.Transform, centroid);
            if (sample.SignedDistanceMeters >= 0)
                continue;
            var penetration = -sample.SignedDistanceMeters;
            var force = sample.OutwardNormal * (settings.PenaltyStiffnessNPerM * penetration);
            result.Add(new(i, force, centroid, penetration));
        }
        return result;
    }
}
