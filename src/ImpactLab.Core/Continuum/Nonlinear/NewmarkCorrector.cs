namespace ImpactLab.Core.Continuum.Nonlinear;

public static class NewmarkCorrector
{
    public static void Correct(
        ReadOnlySpan<double> up,
        ReadOnlySpan<double> vp,
        ReadOnlySpan<double> u,
        double dt,
        double beta,
        double gamma,
        Span<double> v,
        Span<double> a
    )
    {
        var a0 = 1.0 / (beta * dt * dt);
        for (var i = 0; i < u.Length; i++)
        {
            a[i] = (u[i] - up[i]) * a0;
            v[i] = vp[i] + gamma * dt * a[i];
        }
    }
}
