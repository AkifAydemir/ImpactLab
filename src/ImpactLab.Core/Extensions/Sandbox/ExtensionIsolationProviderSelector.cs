namespace ImpactLab.Core.Extensions.Sandbox;

public static class ExtensionIsolationProviderSelector
{
    public static IExtensionIsolationProvider CreateDefault()
    {
        IExtensionIsolationProvider windows = new WindowsJobObjectIsolationProvider();
        return windows.IsAvailable ? windows : new PortableExtensionIsolationProvider();
    }
}
