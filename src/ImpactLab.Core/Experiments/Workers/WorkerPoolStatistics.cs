namespace ImpactLab.Core.Experiments.Workers;

public readonly record struct WorkerPoolStatistics(
    long Submitted,
    long Completed,
    long Failed,
    int Running,
    int Capacity
);
