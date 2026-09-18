namespace ImpactLab.Core.Sparse.Native;

public static class NativeSolverHealthProbe
{
    public static bool IsUsable(NativeSolverProviderDescriptor d) =>
        !string.IsNullOrWhiteSpace(d.LibraryPath)
        && File.Exists(d.LibraryPath)
        && d.ApiVersion.Major == 1;
}
