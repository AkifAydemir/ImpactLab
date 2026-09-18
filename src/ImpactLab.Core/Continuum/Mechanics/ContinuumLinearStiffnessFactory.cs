using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Continuum.Mechanics;

public static class ContinuumLinearStiffnessFactory
{
    public static SparseCsrMatrix Elastic(TetrahedralMesh mesh)
    {
        var b = new SparseTripletBuilder(mesh.Nodes.Count * 3, mesh.Nodes.Count * 3);
        foreach (var e in mesh.Elements)
            ElementTangentScatter.Add(mesh, e, IsotropicElasticity.Matrix(e.Material), b);
        return b.BuildCsr();
    }
}
