namespace ImpactLab.Core.Extensions;

public static class ExtensionCompatibility
{
    public static (bool Compatible, string Message) Check(ExtensionPackageManifest m, Version host)
    {
        if (!Version.TryParse(m.MinimumImpactLabVersion, out var min))
            return (false, "Invalid minimum host version.");
        return host >= min ? (true, "Compatible") : (false, $"Requires ImpactLab {min} or newer.");
    }
}
