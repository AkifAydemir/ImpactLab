using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Dynamics;

public sealed class CentralDifferenceIntegrator : ITransientContinuumIntegrator
{
    public string Id => "central-difference";

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
            var a = new Vec3(
                f[j] / Math.Max(m[j], 1e-30),
                f[j + 1] / Math.Max(m[j + 1], 1e-30),
                f[j + 2] / Math.Max(m[j + 2], 1e-30)
            );
            s.Acceleration[i] = a;
            s.Velocity[i] += a * dt;
            s.Displacement[i] += s.Velocity[i] * dt;
        }
        project?.Invoke(s);
        s.TimeSeconds += dt;
    }
}
