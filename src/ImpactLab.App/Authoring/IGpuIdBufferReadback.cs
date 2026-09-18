namespace ImpactLab.App.Authoring;

public interface IGpuIdBufferReadback
{
    bool IsAvailable { get; }
    bool TryReadObjectId(int x, int y, out int objectId);
}
