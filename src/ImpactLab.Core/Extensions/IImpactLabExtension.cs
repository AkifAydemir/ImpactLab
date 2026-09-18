namespace ImpactLab.Core.Extensions;

public interface IImpactLabExtension
{
    ExtensionManifest Manifest { get; }
    void Register(ExtensionContext context);
}
