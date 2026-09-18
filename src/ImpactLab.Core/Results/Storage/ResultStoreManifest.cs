using ImpactLab.Core.Results.Fields;

namespace ImpactLab.Core.Results.Storage;

public sealed record ResultStoreManifest(
    string RunId,
    int SchemaVersion,
    IReadOnlyList<ResultFieldDescriptor> Fields,
    IReadOnlyDictionary<string, int> FrameCounts,
    long TotalBytes,
    DateTimeOffset CreatedUtc
);
