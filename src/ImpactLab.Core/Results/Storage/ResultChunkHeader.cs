namespace ImpactLab.Core.Results.Storage;

public sealed record ResultChunkHeader(
    ResultChunkKey Key,
    int ItemStart,
    int ItemCount,
    int ComponentCount,
    string Codec,
    long UncompressedBytes,
    long CompressedBytes,
    uint Crc32
);
