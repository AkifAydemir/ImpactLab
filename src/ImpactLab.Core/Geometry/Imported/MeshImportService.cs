namespace ImpactLab.Core.Geometry.Imported;

public sealed record MeshImportResult(
    TriangleMeshAsset Asset,
    MeshRepairReport Inspection,
    string ImporterName,
    MeshImportOptions Options
);

public sealed class MeshImportService
{
    private readonly List<IRawMeshImporter> _importers =
    [
        new ObjMeshImporter(),
        new AutoStlMeshImporter(),
        new StlBinaryMeshImporter(),
    ];

    public MeshImportResult Import(string path, MeshImportOptions? options = null)
    {
        options ??= new();
        var ext = Path.GetExtension(path);
        var importer =
            _importers.FirstOrDefault(x =>
                x.Extensions.Contains(ext, StringComparer.OrdinalIgnoreCase)
            ) ?? throw new NotSupportedException($"Unsupported mesh extension: {ext}");
        var raw = importer.Import(path);
        var asset = MeshUnitNormalizer.Normalize(raw, options);
        return new(asset, MeshInspector.Analyze(asset), importer.GetType().Name, options);
    }
}
