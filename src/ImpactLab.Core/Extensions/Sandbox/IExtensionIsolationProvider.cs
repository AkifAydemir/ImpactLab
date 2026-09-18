using System.Diagnostics;

namespace ImpactLab.Core.Extensions.Sandbox;

public interface IExtensionIsolationProvider
{
    string Name { get; }
    bool IsAvailable { get; }
    IExtensionIsolationLease Attach(Process process, ExtensionSandboxPolicy policy);
}
