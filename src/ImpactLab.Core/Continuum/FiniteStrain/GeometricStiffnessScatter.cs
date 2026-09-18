using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Continuum.FiniteStrain;

public static class GeometricStiffnessScatter
{
    public static void Add(
        TetrahedralMesh mesh,
        TetraElement element,
        Matrix3 stress,
        SparseTripletBuilder triplets
    )
    {
        var stiffness =
            Math.Max(0, stress.Trace)
            * Math.Max(TetraElementGeometry.Volume(mesh, element), 1e-18)
            / 12.0;
        var nodeIds = new[] { element.A, element.B, element.C, element.D };

        foreach (var rowNode in nodeIds)
        {
            foreach (var columnNode in nodeIds)
            {
                for (var dimension = 0; dimension < 3; dimension++)
                {
                    triplets.Add(
                        rowNode * 3 + dimension,
                        columnNode * 3 + dimension,
                        (rowNode == columnNode ? 3 : -1) * stiffness
                    );
                }
            }
        }
    }
}
