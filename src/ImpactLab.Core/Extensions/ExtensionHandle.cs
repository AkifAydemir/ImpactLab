namespace ImpactLab.Core.Extensions;

public sealed record ExtensionHandle(
    ExtensionPackage Package,
    IImpactLabExtension Instance,
    IsolatedExtensionLoadContext LoadContext
)
{
    public string Id => Package.Manifest.Id;
}
