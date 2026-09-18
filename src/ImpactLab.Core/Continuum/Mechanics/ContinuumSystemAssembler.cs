using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Continuum.Mechanics;

public static class ContinuumSystemAssembler
{
    public static CsrMatrix AssembleStiffness(
        TetrahedralMesh mesh,
        Func<TetraElement, double>? stiffnessScale = null
    )
    {
        var map = new ContinuumDofMap(mesh.Nodes.Count);
        var t = new SparseTripletBuilder();
        foreach (var e in mesh.Elements)
        {
            var ke = TetraElementStiffness.Build(mesh, e, stiffnessScale?.Invoke(e) ?? 1.0);
            var dofs = map.ElementDofs(e);
            for (var i = 0; i < 12; i++)
            for (var j = 0; j < 12; j++)
                t.Add(dofs[i], dofs[j], ke[i, j]);
        }
        return t.Build(map.DofCount, map.DofCount);
    }
}
