namespace ImpactLab.Core.Extensions;

public sealed class ExtensionHost
{
    private readonly ExtensionContext _context;
    private readonly List<ExtensionHandle> _loaded = [];

    public ExtensionHost(ExtensionContext context) => _context = context;

    public IReadOnlyList<ExtensionHandle> Loaded => _loaded;

    public ExtensionHandle Load(ExtensionPackage package, Version hostVersion)
    {
        var compatibility = ExtensionCompatibility.Check(package.Manifest, hostVersion);
        if (!compatibility.Compatible)
            throw new InvalidOperationException(compatibility.Message);
        var alc = new IsolatedExtensionLoadContext(package.EntryAssemblyPath);
        try
        {
            var asm = alc.LoadFromAssemblyPath(package.EntryAssemblyPath);
            var type = package.Manifest.EntryType is not null
                ? asm.GetType(package.Manifest.EntryType, true)
                : asm.GetTypes()
                    .First(t =>
                        typeof(IImpactLabExtension).IsAssignableFrom(t)
                        && !t.IsAbstract
                        && !t.IsInterface
                    );
            var instance = (IImpactLabExtension)Activator.CreateInstance(type!)!;
            instance.Register(_context);
            var handle = new ExtensionHandle(package, instance, alc);
            _loaded.Add(handle);
            return handle;
        }
        catch
        {
            alc.Unload();
            throw;
        }
    }

    public void Unload(string id)
    {
        var h = _loaded.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (h is null)
            return;
        _loaded.Remove(h);
        if (h.Instance is IDisposable d)
            d.Dispose();
        h.LoadContext.Unload();
    }
}
