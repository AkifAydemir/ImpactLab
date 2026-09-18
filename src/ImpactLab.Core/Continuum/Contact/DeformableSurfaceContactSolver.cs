using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Contact;

public sealed record NodeTriangleContact(
    int NodeId,
    int TriangleId,
    Vec3 Point,
    Vec3 Normal,
    double Penetration,
    double U,
    double V,
    double W
);

public sealed class DeformableSurfaceContactSolver
{
    public IReadOnlyList<NodeTriangleContact> Detect(
        SurfaceMesh target,
        IReadOnlyList<Vec3> queryNodes,
        double radius
    )
    {
        var bvh = new SurfaceTriangleBvh(target);
        var candidates = new List<int>();
        var contacts = new List<NodeTriangleContact>();
        for (var n = 0; n < queryNodes.Count; n++)
        {
            bvh.Query(queryNodes[n], radius, candidates);
            foreach (var i in candidates)
            {
                var t = target.Triangles[i];
                var a = target.Volume.Nodes[t.A].Position;
                var b = target.Volume.Nodes[t.B].Position;
                var c = target.Volume.Nodes[t.C].Position;
                var q = TriangleClosestPoint.Evaluate(queryNodes[n], a, b, c);
                if (q.Distance >= radius)
                    continue;
                var normal = Vec3.Cross(b - a, c - a).Normalized();
                contacts.Add(new(n, i, q.Point, normal, radius - q.Distance, q.U, q.V, q.W));
            }
        }
        return contacts;
    }
}
