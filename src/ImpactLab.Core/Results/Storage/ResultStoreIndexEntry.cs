namespace ImpactLab.Core.Results.Storage;

public sealed record ResultStoreIndexEntry(
    string FieldId,
    int FrameIndex,
    string Path,
    int ItemCount,
    int Components,
    long Bytes
);
