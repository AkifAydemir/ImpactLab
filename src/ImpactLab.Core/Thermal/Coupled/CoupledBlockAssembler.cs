using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Thermal.Coupled;

public static class CoupledBlockAssembler
{
    public static SparseCsrMatrix Assemble(CoupledDofLayout l, CoupledBlockSystem b)
    {
        var t = new SparseTripletBuilder(l.TotalDofs, l.TotalDofs);
        Scatter(b.Kuu, t, (r) => MechanicalGlobal(l, r), (c) => MechanicalGlobal(l, c));
        Scatter(b.KTT, t, (r) => ThermalGlobal(l, r), (c) => ThermalGlobal(l, c));
        Scatter(b.KuT, t, (r) => MechanicalGlobal(l, r), (c) => ThermalGlobal(l, c));
        Scatter(b.KTu, t, (r) => ThermalGlobal(l, r), (c) => MechanicalGlobal(l, c));
        return t.BuildCsr();
    }

    static int MechanicalGlobal(CoupledDofLayout l, int dof)
    {
        var n = dof / 3;
        return n * 4 + dof % 3;
    }

    static int ThermalGlobal(CoupledDofLayout l, int n) => n * 4 + 3;

    static void Scatter(
        SparseCsrMatrix m,
        SparseTripletBuilder t,
        Func<int, int> rmap,
        Func<int, int> cmap
    )
    {
        for (var r = 0; r < m.RowCount; r++)
        for (var k = m.RowPointers[r]; k < m.RowPointers[r + 1]; k++)
            t.Add(rmap(r), cmap(m.ColumnIndices[k]), m.Values[k]);
    }
}
