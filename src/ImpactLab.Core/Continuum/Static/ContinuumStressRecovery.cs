using ImpactLab.Core.Continuum.Mechanics;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Static;

public static class ContinuumStressRecovery
{
    public static (StrainTensor6[] Strain, StressTensor6[] Stress) Recover(
        TetrahedralMesh mesh,
        ReadOnlySpan<double> u
    )
    {
        var strains = new StrainTensor6[mesh.Elements.Count];
        var stresses = new StressTensor6[mesh.Elements.Count];
        var map = new ContinuumDofMap(mesh.Nodes.Count);
        foreach (var e in mesh.Elements)
        {
            var B = TetraStrainDisplacement.Build(
                mesh.Nodes[e.A].Position,
                mesh.Nodes[e.B].Position,
                mesh.Nodes[e.C].Position,
                mesh.Nodes[e.D].Position
            );
            var dofs = map.ElementDofs(e);
            var eps = new double[6];
            for (var i = 0; i < 6; i++)
            for (var j = 0; j < 12; j++)
                eps[i] += B[i, j] * u[dofs[j]];
            var D = IsotropicElasticity.Matrix(e.Material);
            var s = new double[6];
            for (var i = 0; i < 6; i++)
            for (var j = 0; j < 6; j++)
                s[i] += D[i, j] * eps[j];
            strains[e.Id] = new(eps[0], eps[1], eps[2], eps[3], eps[4], eps[5]);
            stresses[e.Id] = new(s[0], s[1], s[2], s[3], s[4], s[5]);
        }
        return (strains, stresses);
    }
}
