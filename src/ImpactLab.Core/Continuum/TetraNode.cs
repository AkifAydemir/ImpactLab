using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum;

public sealed record TetraNode
{
    public TetraNode(int id, Vec3 position, string partId, MaterialDefinition material)
    {
        Id = id;
        Position = position;
        PartId = partId;
        Material = material;
    }

    public int Id { get; init; }
    public Vec3 Position { get; set; }
    public string PartId { get; init; }
    public MaterialDefinition Material { get; init; }
}
