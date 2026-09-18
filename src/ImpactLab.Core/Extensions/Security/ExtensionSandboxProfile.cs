namespace ImpactLab.Core.Extensions.Security;

public sealed record ExtensionSandboxProfile(
    bool AllowNetwork = false,
    bool AllowChildProcess = false,
    bool AllowNativeCode = false,
    bool ReadOnlyWorkspace = true,
    long MemoryLimitBytes = 512L * 1024 * 1024,
    TimeSpan CpuTimeLimit = default
);
