namespace ImpactLab.Core.Extensions.Sandbox;

public sealed record ExtensionSandboxPolicy(
    TimeSpan StartupTimeout,
    long MaxWorkingSetBytes,
    int MaxCpuPercent,
    bool KillOnHostExit = true,
    bool DisableNetworkByDefault = true
);
