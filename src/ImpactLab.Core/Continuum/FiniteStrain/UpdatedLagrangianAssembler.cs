using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Continuum.FiniteStrain;

public sealed class UpdatedLagrangianAssembler
{
    public SparseCsrMatrix AssembleGeometricStiffness(
        TetrahedralMesh mesh,
        IReadOnlyList<FiniteStrainElementState> state
    )
    {
        var t = new SparseTripletBuilder(mesh.Nodes.Count * 3, mesh.Nodes.Count * 3);
        foreach (var e in mesh.Elements)
            GeometricStiffnessScatter.Add(mesh, e, state[e.Id].CauchyStress, t);
        return t.BuildCsr();
    }
}
