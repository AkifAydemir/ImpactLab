using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Meshing;

public sealed class MeshNode
{
    public MeshNode(
        int id,
        string partId,
        int gridX,
        int gridY,
        int gridZ,
        Vec3 restPosition,
        double massKg,
        double cellSize,
        MaterialDefinition material
    )
    {
        if (string.IsNullOrWhiteSpace(partId))
            throw new ArgumentException("Part id cannot be empty.", nameof(partId));
        Id = id;
        PartId = partId;
        GridX = gridX;
        GridY = gridY;
        GridZ = gridZ;
        RestPosition = restPosition;
        MassKg = massKg;
        CellSize = cellSize;
        Material = material;
    }

    public int Id { get; }
    public string PartId { get; }
    public int GridX { get; }
    public int GridY { get; }
    public int GridZ { get; }
    public Vec3 RestPosition { get; }
    public double MassKg { get; }
    public double CellSize { get; }
    public MaterialDefinition Material { get; }
}
