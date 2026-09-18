using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Continuum.Mechanics;

public sealed record ContinuumDirichletDof(int Dof, double Value);

public static class ContinuumBoundaryAssembler
{
    public static (CsrMatrix Matrix, double[] Rhs) Apply(
        CsrMatrix a,
        double[] rhs,
        IReadOnlyList<ContinuumDirichletDof> fixedDofs
    )
    {
        var fixedMap = fixedDofs.ToDictionary(x => x.Dof, x => x.Value);
        var t = new SparseTripletBuilder();
        var b = (double[])rhs.Clone();
        for (var r = 0; r < a.Rows; r++)
        {
            if (fixedMap.TryGetValue(r, out var prescribed))
            {
                t.Add(r, r, 1);
                b[r] = prescribed;
                continue;
            }
            for (var k = a.RowOffsets[r]; k < a.RowOffsets[r + 1]; k++)
            {
                var c = a.ColumnIndices[k];
                var v = a.Values[k];
                if (fixedMap.TryGetValue(c, out var u))
                    b[r] -= v * u;
                else
                    t.Add(r, c, v);
            }
        }
        return (t.Build(a.Rows, a.Columns), b);
    }
}
