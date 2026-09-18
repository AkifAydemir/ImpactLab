namespace ImpactLab.Core.Continuum.Plasticity;

public static class ConsistentTangentProjector
{
    public static double[,] Symmetrize(double[,] d)
    {
        var n = d.GetLength(0);
        var r = (double[,])d.Clone();
        for (var i = 0; i < n; i++)
        for (var j = i + 1; j < n; j++)
            r[i, j] = r[j, i] = 0.5 * (d[i, j] + d[j, i]);
        return r;
    }
}
