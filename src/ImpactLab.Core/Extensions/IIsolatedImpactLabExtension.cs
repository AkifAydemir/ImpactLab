namespace ImpactLab.Core.Extensions;

public interface IIsolatedImpactLabExtension
{
    ExtensionManifest Manifest { get; }
    ValueTask<IsolatedExtensionResponse> InvokeAsync(
        IsolatedExtensionRequest request,
        CancellationToken ct = default
    );
}
