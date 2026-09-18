using ImpactLab.Core.Continuum;
using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Thermal.Coupled;

public static class ThermoMechanicalDissipationJacobian
{
    public static SparseCsrMatrix Approximate(TetrahedralMesh mesh, double factor)
    {
        var t = new SparseTripletBuilder(mesh.Nodes.Count, mesh.Nodes.Count * 3);
        foreach (var e in mesh.Elements)
        foreach (var n in new[] { e.A, e.B, e.C, e.D })
            for (var d = 0; d < 3; d++)
                t.Add(
                    n,
                    n * 3 + d,
                    factor * Math.Max(TetraElementGeometry.Volume(mesh, e), 1e-18) / 12.0
                );
        return t.BuildCsr();
    }
}
