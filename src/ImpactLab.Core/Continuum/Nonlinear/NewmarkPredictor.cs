namespace ImpactLab.Core.Continuum.Nonlinear;

public static class NewmarkPredictor
{
    public static void Predict(
        ReadOnlySpan<double> u,
        ReadOnlySpan<double> v,
        ReadOnlySpan<double> a,
        double dt,
        double beta,
        double gamma,
        Span<double> up,
        Span<double> vp
    )
    {
        for (var i = 0; i < u.Length; i++)
        {
            up[i] = u[i] + dt * v[i] + dt * dt * (0.5 - beta) * a[i];
            vp[i] = v[i] + dt * (1 - gamma) * a[i];
        }
    }
}
