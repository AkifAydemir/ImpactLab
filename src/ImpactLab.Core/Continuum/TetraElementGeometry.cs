using ImpactLab.Core.Continuum.Mechanics;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum;

public static class TetraElementGeometry
{
    public static double SignedVolume(in Vec3 a, in Vec3 b, in Vec3 c, in Vec3 d) =>
        Vec3.Dot(b - a, Vec3.Cross(c - a, d - a)) / 6.0;

    public static double Volume(TetrahedralMesh mesh, TetraElement element) =>
        Math.Abs(
            SignedVolume(
                mesh.Nodes[element.A].Position,
                mesh.Nodes[element.B].Position,
                mesh.Nodes[element.C].Position,
                mesh.Nodes[element.D].Position
            )
        );

    public static double[,] StrainDisplacement(TetrahedralMesh mesh, TetraElement element) =>
        TetraStrainDisplacement.Build(
            mesh.Nodes[element.A].Position,
            mesh.Nodes[element.B].Position,
            mesh.Nodes[element.C].Position,
            mesh.Nodes[element.D].Position
        );

    public static Vec3 Centroid(in Vec3 a, in Vec3 b, in Vec3 c, in Vec3 d) =>
        (a + b + c + d) * 0.25;

    public static double EdgeMin(params Vec3[] p)
    {
        var m = double.MaxValue;
        for (var i = 0; i < 4; i++)
        for (var j = i + 1; j < 4; j++)
            m = Math.Min(m, (p[i] - p[j]).Length);
        return m;
    }

    public static double EdgeMax(params Vec3[] p)
    {
        var m = 0.0;
        for (var i = 0; i < 4; i++)
        for (var j = i + 1; j < 4; j++)
            m = Math.Max(m, (p[i] - p[j]).Length);
        return m;
    }
}
