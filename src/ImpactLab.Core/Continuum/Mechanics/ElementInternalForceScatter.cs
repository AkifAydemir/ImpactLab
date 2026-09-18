namespace ImpactLab.Core.Continuum.Mechanics;

public static class ElementInternalForceScatter
{
    public static void Add(
        TetrahedralMesh mesh,
        TetraElement e,
        StressTensor6 stress,
        double[] global
    )
    {
        var b = TetraElementGeometry.StrainDisplacement(mesh, e);
        var s = stress.ToArray();
        var v = TetraElementGeometry.Volume(mesh, e);
        for (var i = 0; i < 12; i++)
        {
            double f = 0;
            for (var a = 0; a < 6; a++)
                f += b[a, i] * s[a];
            global[e.Dof(i)] += f * v;
        }
    }
}
