using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Mechanics;

public static class ContinuumLoadVectorAssembler
{
    public static double[] BodyForce(TetrahedralMesh mesh, Vec3 acceleration)
    {
        var f = new double[mesh.Nodes.Count * 3];
        foreach (var e in mesh.Elements)
        {
            var vol = Math.Abs(
                TetraElementGeometry.SignedVolume(
                    mesh.Nodes[e.A].Position,
                    mesh.Nodes[e.B].Position,
                    mesh.Nodes[e.C].Position,
                    mesh.Nodes[e.D].Position
                )
            );
            var mass = e.Material.DensityKgPerM3 * vol;
            foreach (var n in e.Nodes())
            {
                var q = acceleration * (mass / 4.0);
                f[n * 3] += q.X;
                f[n * 3 + 1] += q.Y;
                f[n * 3 + 2] += q.Z;
            }
        }
        return f;
    }

    public static void AddNodal(double[] f, int node, Vec3 force)
    {
        f[node * 3] += force.X;
        f[node * 3 + 1] += force.Y;
        f[node * 3 + 2] += force.Z;
    }
}
