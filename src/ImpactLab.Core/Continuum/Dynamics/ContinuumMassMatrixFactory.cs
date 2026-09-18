using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Continuum.Dynamics;

public static class ContinuumMassMatrixFactory
{
    public static SparseCsrMatrix LumpedCsr(TetrahedralMesh mesh)
    {
        var d = ContinuumMassAssembler.Lumped(mesh);
        var b = new SparseTripletBuilder(d.Length, d.Length);
        for (var i = 0; i < d.Length; i++)
            b.Add(i, i, d[i]);
        return b.BuildCsr();
    }
}
