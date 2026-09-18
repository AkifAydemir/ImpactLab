using ImpactLab.Core.Materials;

namespace ImpactLab.Core.Continuum;

public sealed record TetraElement(
    int Id,
    int A,
    int B,
    int C,
    int D,
    string PartId,
    MaterialDefinition Material
)
{
    public int this[int i] =>
        i switch
        {
            0 => A,
            1 => B,
            2 => C,
            3 => D,
            _ => throw new ArgumentOutOfRangeException(nameof(i)),
        };

    public int[] Nodes() => [A, B, C, D];

    public int Dof(int localDof) => this[localDof / 3] * 3 + localDof % 3;

    public double[] EdgeLengths(TetrahedralMesh mesh)
    {
        var nodes = Nodes().Select(index => mesh.Nodes[index].Position).ToArray();
        var lengths = new List<double>(6);
        for (var first = 0; first < nodes.Length; first++)
        for (var second = first + 1; second < nodes.Length; second++)
            lengths.Add((nodes[first] - nodes[second]).Length);
        return lengths.ToArray();
    }
}
