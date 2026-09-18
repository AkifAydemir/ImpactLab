using ImpactLab.Core.Continuum.Mechanics;
using ImpactLab.Core.Materials;

namespace ImpactLab.Core.Continuum.Plasticity;

public sealed class J2ReturnMapper
{
    public ReturnMappingResult Update(
        MaterialDefinition m,
        StrainTensor6 total,
        PlasticState old,
        IHardeningLaw law
    )
    {
        var e = m.YoungModulusPa;
        var nu = m.PoissonRatio;
        var g = e / (2 * (1 + nu));
        var elastic = StrainMath.Subtract(total, old.PlasticStrain);
        var trial = Apply(IsotropicElasticity.Matrix(m), elastic);
        var q = StressMath.J2Norm(trial);
        var sy = law.YieldStress(old.EquivalentPlasticStrain);
        if (q <= sy * (1 + 1e-10))
            return new(trial, old with { YieldStressPa = sy }, false, 0, 1, 0);
        var h = Math.Max(0, law.Tangent(old.EquivalentPlasticStrain));
        var dg = (q - sy) / Math.Max(3 * g + h, 1e-30);
        var dev = StressMath.Deviator(trial);
        var scale = Math.Max(0, 1 - 3 * g * dg / Math.Max(q, 1e-30));
        var corrected = StressMath.Add(
            StressMath.Scale(dev, scale),
            new StressTensor6(
                StressMath.Mean(trial),
                StressMath.Mean(trial),
                StressMath.Mean(trial),
                0,
                0,
                0
            )
        );
        var dep = Flow(dev, dg, q);
        var eq = old.EquivalentPlasticStrain + dg;
        var next = new PlasticState(
            StrainMath.Add(old.PlasticStrain, dep),
            eq,
            law.YieldStress(eq)
        );
        return new(
            corrected,
            next,
            true,
            dg,
            Math.Clamp(h / Math.Max(h + 3 * g, 1e-30), 0.001, 1),
            Math.Max(0, next.YieldStressPa * dg)
        );
    }

    private static StressTensor6 Apply(double[,] d, StrainTensor6 e)
    {
        var x = new[] { e.XX, e.YY, e.ZZ, e.XY, e.YZ, e.ZX };
        var s = new double[6];
        for (var i = 0; i < 6; i++)
        for (var j = 0; j < 6; j++)
            s[i] += d[i, j] * x[j];
        return new(s[0], s[1], s[2], s[3], s[4], s[5]);
    }

    private static StrainTensor6 Flow(StressTensor6 dev, double dg, double q)
    {
        var k = 1.5 * dg / Math.Max(q, 1e-30);
        return new(
            dev.XX * k,
            dev.YY * k,
            dev.ZZ * k,
            2 * dev.XY * k,
            2 * dev.YZ * k,
            2 * dev.ZX * k
        );
    }
}
