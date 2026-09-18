namespace ImpactLab.Core.Sparse.Parallel;

public sealed class ParallelAssemblyBuffer
{
    public List<(int Row, int Col, double Value)> Triplets { get; } = [];
    public double[] Residual { get; }

    public ParallelAssemblyBuffer(int dofs) => Residual = new double[dofs];
}
