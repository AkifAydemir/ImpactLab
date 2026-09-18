using System.Runtime.InteropServices;

namespace ImpactLab.Core.Sparse;

internal static class NativeSparseLibrary
{
    public const string LibraryName = "impactlab_sparse";

    [DllImport(
        LibraryName,
        EntryPoint = "impactlab_csr_solve",
        CallingConvention = CallingConvention.Cdecl
    )]
    internal static extern int Solve(
        int n,
        int nnz,
        int[] rowOffsets,
        int[] columns,
        double[] values,
        double[] rhs,
        double[] x,
        double tolerance,
        int maxIterations,
        out int iterations,
        out double residual
    );

    public static bool Probe()
    {
        try
        {
            return NativeLibrary.TryLoad(LibraryName, out var h) && Release(h);
        }
        catch
        {
            return false;
        }
        static bool Release(nint h)
        {
            NativeLibrary.Free(h);
            return true;
        }
    }
}
