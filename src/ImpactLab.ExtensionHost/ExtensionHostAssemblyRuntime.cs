using ImpactLab.Core.Extensions;

namespace ImpactLab.ExtensionHost;

public sealed class ExtensionHostAssemblyRuntime : IDisposable
{
    private readonly IsolatedExtensionLoadContext _alc;
    private readonly IIsolatedImpactLabExtension? _isolated;
    public ExtensionManifest? Manifest => _isolated?.Manifest;

    public ExtensionHostAssemblyRuntime(
        string entryAssembly,
        string? entryType,
        bool allowNativeCode
    )
    {
        _alc = new(entryAssembly, allowNativeCode);
        var asm = _alc.LoadFromAssemblyPath(entryAssembly);
        Type? type = entryType is not null
            ? asm.GetType(entryType, true)
            : asm.GetTypes()
                .FirstOrDefault(t =>
                    typeof(IIsolatedImpactLabExtension).IsAssignableFrom(t)
                    && !t.IsAbstract
                    && !t.IsInterface
                );
        if (type is not null && !typeof(IIsolatedImpactLabExtension).IsAssignableFrom(type))
            throw new InvalidOperationException(
                "Isolated extension entry type must implement IIsolatedImpactLabExtension."
            );
        if (type is not null)
            _isolated = (IIsolatedImpactLabExtension)Activator.CreateInstance(type)!;
    }

    public ValueTask<IsolatedExtensionResponse> InvokeAsync(
        IsolatedExtensionRequest request,
        CancellationToken ct
    ) =>
        _isolated is null
            ? ValueTask.FromResult(
                new IsolatedExtensionResponse(
                    false,
                    "{}",
                    "Extension does not implement IIsolatedImpactLabExtension."
                )
            )
            : _isolated.InvokeAsync(request, ct);

    public void Dispose()
    {
        if (_isolated is IDisposable d)
            d.Dispose();
        _alc.Unload();
    }
}
