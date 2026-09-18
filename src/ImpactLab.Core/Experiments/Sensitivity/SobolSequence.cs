namespace ImpactLab.Core.Experiments.Sensitivity;

public static class SobolSequence
{
    public static double[,] Generate(int count, int dimensions)
    {
        var x = new double[count, dimensions];
        for (var i = 0; i < count; i++)
        for (var d = 0; d < dimensions; d++)
            x[i, d] = VanDerCorput(i + 1, 2 + d);
        return x;
    }

    private static double VanDerCorput(int n, int b)
    {
        double q = 0,
            bk = 1.0 / b;
        while (n > 0)
        {
            q += (n % b) * bk;
            n /= b;
            bk /= b;
        }
        return q;
    }
}
