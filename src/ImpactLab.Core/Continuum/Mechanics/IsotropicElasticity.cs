using ImpactLab.Core.Materials;

namespace ImpactLab.Core.Continuum.Mechanics;

public static class IsotropicElasticity
{
    public static double[,] Matrix(MaterialDefinition m, double stiffnessScale = 1.0)
    {
        var e = m.YoungModulusPa * stiffnessScale;
        var nu = m.PoissonRatio;
        var c = e / ((1 + nu) * (1 - 2 * nu));
        var d = new double[6, 6];
        var a = c * (1 - nu);
        var b = c * nu;
        var s = c * (1 - 2 * nu) / 2;
        d[0, 0] = d[1, 1] = d[2, 2] = a;
        d[0, 1] = d[0, 2] = d[1, 0] = d[1, 2] = d[2, 0] = d[2, 1] = b;
        d[3, 3] = d[4, 4] = d[5, 5] = s;
        return d;
    }
}
