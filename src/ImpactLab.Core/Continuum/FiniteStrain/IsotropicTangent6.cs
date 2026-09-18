namespace ImpactLab.Core.Continuum.FiniteStrain;

public static class IsotropicTangent6
{
    public static double[,] Build(double lambda, double mu)
    {
        var d = new double[6, 6];
        for (var i = 0; i < 3; i++)
        for (var j = 0; j < 3; j++)
            d[i, j] = lambda;
        for (var i = 0; i < 3; i++)
            d[i, i] += 2 * mu;
        for (var i = 3; i < 6; i++)
            d[i, i] = mu;
        return d;
    }
}
