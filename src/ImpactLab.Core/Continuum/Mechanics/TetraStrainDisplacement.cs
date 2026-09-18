using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Mechanics;

public static class TetraStrainDisplacement
{
    public static double[,] Build(in Vec3 a, in Vec3 b, in Vec3 c, in Vec3 d)
    {
        var m = new double[,]
        {
            { 1, a.X, a.Y, a.Z },
            { 1, b.X, b.Y, b.Z },
            { 1, c.X, c.Y, c.Z },
            { 1, d.X, d.Y, d.Z },
        };
        var inv = Invert4(m);
        var B = new double[6, 12];
        for (var n = 0; n < 4; n++)
        {
            var bx = inv[1, n];
            var by = inv[2, n];
            var bz = inv[3, n];
            var j = n * 3;
            B[0, j] = bx;
            B[1, j + 1] = by;
            B[2, j + 2] = bz;
            B[3, j] = by;
            B[3, j + 1] = bx;
            B[4, j + 1] = bz;
            B[4, j + 2] = by;
            B[5, j] = bz;
            B[5, j + 2] = bx;
        }
        return B;
    }

    private static double[,] Invert4(double[,] a)
    {
        var n = 4;
        var x = new double[n, n * 2];
        for (var i = 0; i < n; i++)
        for (var j = 0; j < n; j++)
        {
            x[i, j] = a[i, j];
            x[i, j + n] = i == j ? 1 : 0;
        }
        for (var p = 0; p < n; p++)
        {
            var best = p;
            for (var r = p + 1; r < n; r++)
                if (Math.Abs(x[r, p]) > Math.Abs(x[best, p]))
                    best = r;
            if (Math.Abs(x[best, p]) < 1e-20)
                throw new InvalidOperationException("Singular tetra coordinate matrix.");
            if (best != p)
                for (var j = 0; j < 2 * n; j++)
                    (x[p, j], x[best, j]) = (x[best, j], x[p, j]);
            var pivot = x[p, p];
            for (var j = 0; j < 2 * n; j++)
                x[p, j] /= pivot;
            for (var r = 0; r < n; r++)
                if (r != p)
                {
                    var f = x[r, p];
                    for (var j = 0; j < 2 * n; j++)
                        x[r, j] -= f * x[p, j];
                }
        }
        var inv = new double[n, n];
        for (var i = 0; i < n; i++)
        for (var j = 0; j < n; j++)
            inv[i, j] = x[i, j + n];
        return inv;
    }
}
