using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Continuum.Mechanics;

public static class ElementTangentScatter
{
    public static void Add(
        TetrahedralMesh mesh,
        TetraElement e,
        double[,] d,
        SparseTripletBuilder global
    )
    {
        var b = TetraElementGeometry.StrainDisplacement(mesh, e);
        var v = TetraElementGeometry.Volume(mesh, e);
        for (var i = 0; i < 12; i++)
        for (var j = 0; j < 12; j++)
        {
            double s = 0;
            for (var a = 0; a < 6; a++)
            for (var c = 0; c < 6; c++)
                s += b[a, i] * d[a, c] * b[c, j];
            global.Add(e.Dof(i), e.Dof(j), s * v);
        }
    }
}
