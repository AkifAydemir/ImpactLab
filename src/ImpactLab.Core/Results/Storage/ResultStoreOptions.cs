namespace ImpactLab.Core.Results.Storage;

public sealed record ResultStoreOptions(
    int ChunkItems = 65536,
    string Codec = "deflate",
    int CacheFrames = 12,
    bool WriteChecksums = true
);
