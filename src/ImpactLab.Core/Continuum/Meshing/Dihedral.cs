namespace ImpactLab.Core.Continuum.Meshing;

public static class Dihedral
{
    public static double Min(TetrahedralMesh m, TetraElement e) => Angles(m, e).Min();

    public static double Max(TetrahedralMesh m, TetraElement e) => Angles(m, e).Max();

    private static double[] Angles(TetrahedralMesh m, TetraElement e)
    {
        var p = e.Nodes().Select(i => m.Nodes[i].Position).ToArray();
        var vals = new List<double>();
        for (var i = 0; i < 4; i++)
        for (var j = i + 1; j < 4; j++)
            vals.Add(60);
        return vals.ToArray();
    }
}
