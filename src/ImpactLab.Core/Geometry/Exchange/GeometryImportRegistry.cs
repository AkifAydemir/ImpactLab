using ImpactLab.Core.Geometry.Imported;

namespace ImpactLab.Core.Geometry.Exchange;

public sealed class GeometryImportRegistry
{
    private readonly List<IGeometryExchangeImporter> _i = [];

    public void Register(IGeometryExchangeImporter i) => _i.Add(i);

    public TriangleMeshAsset Import(string path)
    {
        var ext = Path.GetExtension(path);
        var i =
            _i.LastOrDefault(x => x.Extensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
            ?? throw new NotSupportedException($"No geometry importer for {ext}.");
        return i.Import(path);
    }

    public IReadOnlyList<string> SupportedExtensions() =>
        _i.SelectMany(x => x.Extensions)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToArray();
}
