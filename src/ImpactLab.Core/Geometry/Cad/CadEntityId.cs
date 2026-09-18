namespace ImpactLab.Core.Geometry.Cad;

public readonly record struct CadEntityId(Guid Value)
{
    public static CadEntityId New() => new(Guid.NewGuid());
}
