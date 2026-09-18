using ImpactLab.Core.Continuum;
using ImpactLab.Core.Continuum.Mechanics;

namespace ImpactLab.Core.Continuum.Dynamics;

public static class ContinuumMassAssembler
{
    public static double[] Lumped(TetrahedralMesh mesh)
    {
        var m = new double[mesh.Nodes.Count * 3];
        foreach (var e in mesh.Elements)
        {
            var v = TetraElementGeometry.Volume(mesh, e);
            var share = e.Material.DensityKgPerM3 * v / 4.0;
            foreach (var n in e.Nodes())
                for (var a = 0; a < 3; a++)
                    m[n * 3 + a] += share;
        }
        return m;
    }

    public static double[,] ConsistentElement(TetrahedralMesh mesh, TetraElement e)
    {
        var total = e.Material.DensityKgPerM3 * TetraElementGeometry.Volume(mesh, e);
        var a = total / 20.0;
        var m = new double[12, 12];
        for (var i = 0; i < 4; i++)
        for (var j = 0; j < 4; j++)
        for (var k = 0; k < 3; k++)
            m[i * 3 + k, j * 3 + k] = a * (i == j ? 2 : 1);
        return m;
    }
}
