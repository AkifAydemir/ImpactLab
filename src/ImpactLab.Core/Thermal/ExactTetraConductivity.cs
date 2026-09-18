using ImpactLab.Core.Continuum;
using ImpactLab.Core.Continuum.Mechanics;

namespace ImpactLab.Core.Thermal;

public static class ExactTetraConductivity
{
    public static double[,] Build(TetrahedralMesh mesh, TetraElement e, double conductivity)
    {
        var p = e.Nodes().Select(i => mesh.Nodes[i].Position).ToArray();
        var b = TetraStrainDisplacement.Build(p[0], p[1], p[2], p[3]);
        var v = TetraElementGeometry.Volume(mesh, e);
        var k = new double[4, 4];
        for (var i = 0; i < 4; i++)
        for (var j = 0; j < 4; j++)
        {
            var gi = new[] { b[0, i * 3], b[1, i * 3 + 1], b[2, i * 3 + 2] };
            var gj = new[] { b[0, j * 3], b[1, j * 3 + 1], b[2, j * 3 + 2] };
            k[i, j] = conductivity * v * (gi[0] * gj[0] + gi[1] * gj[1] + gi[2] * gj[2]);
        }
        return k;
    }
}
