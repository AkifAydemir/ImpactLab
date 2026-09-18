namespace ImpactLab.Core.Extensions;

public sealed record ExtensionManifest(
    string Id,
    string DisplayName,
    Version Version,
    ExtensionCapability Capabilities,
    string Vendor = "Unknown",
    string Description = ""
)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id) || string.IsNullOrWhiteSpace(DisplayName))
            throw new InvalidOperationException("Extension id and display name are required.");
    }
}
