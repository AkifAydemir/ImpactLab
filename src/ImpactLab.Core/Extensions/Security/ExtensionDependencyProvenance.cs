namespace ImpactLab.Core.Extensions.Security;

public sealed record ExtensionDependencyProvenance(
    string Name,
    string Version,
    string Sha256,
    string Source,
    bool IsDirect
);
