namespace ImpactLab.Core.Continuum.Mechanics;

public sealed record TetraElementKinematics(StrainTensor6 Strain)
{
    public static TetraElementKinematics FromDisplacement(
        TetrahedralMesh mesh,
        TetraElement e,
        ReadOnlySpan<double> u
    )
    {
        var b = TetraElementGeometry.StrainDisplacement(mesh, e);
        var ev = new double[6];
        for (var r = 0; r < 6; r++)
        for (var c = 0; c < 12; c++)
            ev[r] += b[r, c] * u[e.Dof(c)];
        return new TetraElementKinematics(
            new StrainTensor6(ev[0], ev[1], ev[2], ev[3], ev[4], ev[5])
        );
    }
}
