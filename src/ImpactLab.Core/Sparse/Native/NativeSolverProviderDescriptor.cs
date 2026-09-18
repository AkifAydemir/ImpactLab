namespace ImpactLab.Core.Sparse.Native;

public sealed record NativeSolverProviderDescriptor(
    string Id,
    string DisplayName,
    Version ApiVersion,
    string LibraryPath,
    IReadOnlyList<string> Capabilities
);
