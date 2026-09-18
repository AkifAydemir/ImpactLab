using ImpactLab.Core.Geometry.Imported;

namespace ImpactLab.Core.Geometry.Exchange;

public interface IGeometryExchangeImporter
{
    string Id { get; }
    IReadOnlyCollection<string> Extensions { get; }
    TriangleMeshAsset Import(string path);
}
