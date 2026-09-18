using ImpactLab.Core.Materials;

namespace ImpactLab.Core.Meshing;

public sealed class MeshSpring
{
    public MeshSpring(
        string partId,
        int nodeA,
        int nodeB,
        double restLength,
        double effectiveAreaM2,
        MaterialDefinition material,
        MaterialInterfaceDefinition? materialInterface = null
    )
    {
        PartId = partId;
        NodeA = nodeA;
        NodeB = nodeB;
        RestLength = restLength;
        EffectiveAreaM2 = effectiveAreaM2;
        Material = material;
        Interface = materialInterface;
    }

    public string PartId { get; }
    public int NodeA { get; }
    public int NodeB { get; }
    public double RestLength { get; }
    public double EffectiveAreaM2 { get; }
    public MaterialDefinition Material { get; }
    public MaterialInterfaceDefinition? Interface { get; }
    public bool IsMaterialInterface => Interface is not null;
    public double Damage { get; set; }
    public bool IsBroken => Damage >= 0.999999;
}
