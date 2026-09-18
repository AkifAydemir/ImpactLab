namespace ImpactLab.Core.Experiments.Workers;

public sealed record WorkerHello(string WorkerId, WorkerCapabilities Capabilities, int ProcessId);
