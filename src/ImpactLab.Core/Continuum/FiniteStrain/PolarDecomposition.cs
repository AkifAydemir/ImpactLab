namespace ImpactLab.Core.Continuum.FiniteStrain;

public static class PolarDecomposition
{
    public static PolarDecompositionResult Decompose(
        DeformationGradient3 f,
        int maxIterations = 32,
        double tolerance = 1e-12
    )
    {
        var r = f.Value;
        var residual = double.PositiveInfinity;
        var i = 0;
        for (; i < maxIterations; i++)
        {
            var next = (r + r.Inverse().Transpose()) * 0.5;
            residual = Matrix3Math.Frobenius(next - r);
            r = next;
            if (residual < tolerance)
                break;
        }
        var u = r.Transpose() * f.Value;
        var v = f.Value * r.Transpose();
        return new(r, u, v, i + 1, residual);
    }
}
