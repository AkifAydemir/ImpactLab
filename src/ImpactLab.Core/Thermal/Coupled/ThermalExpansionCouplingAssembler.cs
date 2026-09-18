using ImpactLab.Core.Continuum;
using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Thermal.Coupled;

public static class ThermalExpansionCouplingAssembler
{
    public static SparseCsrMatrix Assemble(
        TetrahedralMesh mesh,
        double alpha,
        double referenceTemperature
    )
    {
        var t = new SparseTripletBuilder(mesh.Nodes.Count * 3, mesh.Nodes.Count);
        foreach (var e in mesh.Elements)
        {
            var k =
                e.Material.YoungModulusPa
                * alpha
                * Math.Max(TetraElementGeometry.Volume(mesh, e), 1e-18)
                / 4.0;
            foreach (var n in new[] { e.A, e.B, e.C, e.D })
                for (var d = 0; d < 3; d++)
                    t.Add(n * 3 + d, n, k);
        }
        return t.BuildCsr();
    }
}
