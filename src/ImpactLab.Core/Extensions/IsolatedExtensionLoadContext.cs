using System.Reflection;
using System.Runtime.Loader;

namespace ImpactLab.Core.Extensions;

public sealed class IsolatedExtensionLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;
    private readonly bool _allowUnmanagedDll;

    public IsolatedExtensionLoadContext(string entryAssembly, bool allowUnmanagedDll = true)
        : base(
            $"ImpactLab.Extension:{Path.GetFileNameWithoutExtension(entryAssembly)}",
            isCollectible: true
        )
    {
        _resolver = new(entryAssembly);
        _allowUnmanagedDll = allowUnmanagedDll;
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var path = _resolver.ResolveAssemblyToPath(assemblyName);
        return path is null ? null : LoadFromAssemblyPath(path);
    }

    protected override nint LoadUnmanagedDll(string name)
    {
        if (!_allowUnmanagedDll)
            return 0;
        var path = _resolver.ResolveUnmanagedDllToPath(name);
        return path is null ? 0 : LoadUnmanagedDllFromPath(path);
    }
}
