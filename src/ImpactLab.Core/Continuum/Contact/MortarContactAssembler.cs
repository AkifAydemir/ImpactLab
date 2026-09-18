using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Continuum.Contact;

public sealed class MortarContactAssembler
{
    public MortarContactContribution Assemble(
        int dofCount,
        IReadOnlyList<ContactManifoldPoint> points,
        ContactMultiplierStore multipliers,
        double penalty
    )
    {
        var r = new double[dofCount];
        var t = new SparseTripletBuilder(dofCount, dofCount);
        double e = 0;
        foreach (var p in points)
        {
            if (p.GapMeters >= 0)
                continue;
            var lambda = Math.Max(
                0,
                multipliers.Get(p.Key).NormalMultiplierN - penalty * p.GapMeters
            );
            var node = p.Key.SlaveEntity;
            for (var d = 0; d < 3; d++)
            {
                var i = node * 3 + d;
                if (i >= dofCount)
                    break;
                var n = d switch
                {
                    0 => p.Normal.X,
                    1 => p.Normal.Y,
                    _ => p.Normal.Z,
                };
                r[i] += lambda * n;
                t.Add(i, i, penalty * n * n);
            }
            e += 0.5 * penalty * p.GapMeters * p.GapMeters;
        }
        return new(r, t.BuildCsr(), e, points.Count(x => x.GapMeters < 0));
    }
}
