namespace ImpactLab.Core.Continuum.Mechanics;

public static class TetraElementStiffness
{
    public static double[,] Build(TetrahedralMesh mesh, TetraElement e, double stiffnessScale = 1.0)
    {
        var a = mesh.Nodes[e.A].Position;
        var b = mesh.Nodes[e.B].Position;
        var c = mesh.Nodes[e.C].Position;
        var d = mesh.Nodes[e.D].Position;
        var B = TetraStrainDisplacement.Build(a, b, c, d);
        var D = IsotropicElasticity.Matrix(e.Material, stiffnessScale);
        var volume = Math.Abs(TetraElementGeometry.SignedVolume(a, b, c, d));
        var db = Multiply(D, B);
        var bt = Transpose(B);
        var k = Multiply(bt, db);
        for (var i = 0; i < 12; i++)
        for (var j = 0; j < 12; j++)
            k[i, j] *= volume;
        return k;
    }

    private static double[,] Multiply(double[,] a, double[,] b)
    {
        var r = new double[a.GetLength(0), b.GetLength(1)];
        for (var i = 0; i < r.GetLength(0); i++)
        for (var k = 0; k < a.GetLength(1); k++)
        for (var j = 0; j < r.GetLength(1); j++)
            r[i, j] += a[i, k] * b[k, j];
        return r;
    }

    private static double[,] Transpose(double[,] a)
    {
        var r = new double[a.GetLength(1), a.GetLength(0)];
        for (var i = 0; i < a.GetLength(0); i++)
        for (var j = 0; j < a.GetLength(1); j++)
            r[j, i] = a[i, j];
        return r;
    }
}
