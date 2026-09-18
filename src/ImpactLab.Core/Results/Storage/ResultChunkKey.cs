namespace ImpactLab.Core.Results.Storage;

public readonly record struct ResultChunkKey(
    string RunId,
    string FieldId,
    int FrameIndex,
    int ChunkIndex
);
