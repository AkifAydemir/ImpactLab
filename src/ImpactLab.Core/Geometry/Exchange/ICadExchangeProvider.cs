namespace ImpactLab.Core.Geometry.Exchange;

public interface ICadExchangeProvider
{
    CadExchangeDescriptor Descriptor { get; }
    Geometry.Imported.TriangleMeshAsset Tessellate(
        string path,
        CadTessellationSettings settings,
        CancellationToken ct = default
    );
}
