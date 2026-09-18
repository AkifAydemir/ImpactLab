using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Dynamics;

public sealed class NewmarkBetaIntegrator : ITransientContinuumIntegrator
{
    public NewmarkBetaIntegrator(double beta = 0.25, double gamma = 0.5)
    {
        if (beta <= 0)
            throw new ArgumentOutOfRangeException(nameof(beta), "Beta must be positive.");
        if (gamma <= 0)
            throw new ArgumentOutOfRangeException(nameof(gamma), "Gamma must be positive.");
        Beta = beta;
        Gamma = gamma;
    }

    public string Id => "newmark-beta";
    public double Beta { get; }
    public double Gamma { get; }

    public void Begin(ContinuumDynamicState state, double[] mass) { }

    public void Step(
        ContinuumDynamicState s,
        ReadOnlySpan<double> f,
        double[] m,
        double dt,
        Action<ContinuumDynamicState>? project = null
    )
    {
        for (var i = 0; i < s.Displacement.Length; i++)
        {
            var j = i * 3;
            var next = new Vec3(
                f[j] / Math.Max(m[j], 1e-30),
                f[j + 1] / Math.Max(m[j + 1], 1e-30),
                f[j + 2] / Math.Max(m[j + 2], 1e-30)
            );
            var old = s.Acceleration[i];
            s.Displacement[i] +=
                s.Velocity[i] * dt + old * (dt * dt * (0.5 - Beta)) + next * (dt * dt * Beta);
            s.Velocity[i] += old * (dt * (1 - Gamma)) + next * (dt * Gamma);
            s.Acceleration[i] = next;
        }
        project?.Invoke(s);
        s.TimeSeconds += dt;
    }
}
