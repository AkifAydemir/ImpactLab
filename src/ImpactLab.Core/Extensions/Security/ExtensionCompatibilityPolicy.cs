namespace ImpactLab.Core.Extensions.Security;

public sealed record ExtensionCompatibilityPolicy(Version ApiVersion, Version MinimumHostVersion)
{
    public bool IsCompatible(ExtensionManifest m, Version host) =>
        Version.TryParse(m.ApiVersion, out var api)
        && api.Major == ApiVersion.Major
        && host >= MinimumHostVersion;
}
