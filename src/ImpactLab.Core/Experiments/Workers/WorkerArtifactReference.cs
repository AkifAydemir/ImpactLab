namespace ImpactLab.Core.Experiments.Workers;

public sealed record WorkerArtifactReference(string Kind, string Path, long Bytes, string Sha256);
