namespace ImpactLab.Core.Sparse.Native;

public sealed record NativeSolveRequest(
    string SolverId,
    int Rows,
    int Columns,
    int[] RowPointers,
    int[] ColumnIndices,
    double[] Values,
    double[] Rhs,
    double RelativeTolerance,
    int MaxIterations
);
