namespace ImpactLab.Core.Experiments.Workers;

public sealed record WorkerCapabilities(
    string WorkerVersion,
    string Runtime,
    string OperatingSystem,
    int ProcessorCount,
    long AvailableMemoryBytes,
    IReadOnlyList<string> BackendIds
);
