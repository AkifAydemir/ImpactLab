using ImpactLab.Core.Continuum;
using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Thermal;

public static class ThermalSystemAssembler
{
    public static (CsrMatrix Capacity, CsrMatrix Conductivity) Assemble(
        TetrahedralMesh mesh,
        ThermalPropertyTable props
    )
    {
        var c = new SparseTripletBuilder();
        var k = new SparseTripletBuilder();
        foreach (var e in mesh.Elements)
        {
            var p = props.Resolve(e.Material.Id);
            var vol = Math.Abs(
                TetraElementGeometry.SignedVolume(
                    mesh.Nodes[e.A].Position,
                    mesh.Nodes[e.B].Position,
                    mesh.Nodes[e.C].Position,
                    mesh.Nodes[e.D].Position
                )
            );
            var lump = e.Material.DensityKgPerM3 * p.SpecificHeatJPerKgK * vol / 4.0;
            foreach (var n in e.Nodes())
                c.Add(n, n, lump);
            var ids = e.Nodes();
            for (var i = 0; i < 4; i++)
            for (var j = i + 1; j < 4; j++)
            {
                var len = (mesh.Nodes[ids[i]].Position - mesh.Nodes[ids[j]].Position).Length;
                var conduct = p.ConductivityWPerMK * vol / Math.Max(len * len, 1e-18) / 6.0;
                k.Add(ids[i], ids[i], conduct);
                k.Add(ids[j], ids[j], conduct);
                k.Add(ids[i], ids[j], -conduct);
                k.Add(ids[j], ids[i], -conduct);
            }
        }
        return (
            c.Build(mesh.Nodes.Count, mesh.Nodes.Count),
            k.Build(mesh.Nodes.Count, mesh.Nodes.Count)
        );
    }
}
