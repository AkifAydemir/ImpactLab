namespace ImpactLab.Core.Extensions.Sandbox;

public interface IExtensionIsolationLease : IDisposable
{
    string Provider { get; }
    IReadOnlyList<string> Diagnostics { get; }
}
