namespace ImpactLab.Core.Geometry.Imported;

public sealed record StlLayerExportResult(
    MeshImportResult Import,
    IReadOnlyList<MeshLayer> Layers,
    IReadOnlyList<string> Paths
);

public sealed class StlLayerExportService
{
    private readonly MeshImportService _importer = new();

    public StlLayerExportResult Export(
        string stlPath,
        string outputDirectory,
        int layerCount,
        MeshLayerAxis axis = MeshLayerAxis.Z,
        MeshImportOptions? options = null
    )
    {
        if (!string.Equals(Path.GetExtension(stlPath), ".stl", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException("Layer export currently accepts STL input.");
        var imported = _importer.Import(stlPath, options);
        var layers = MeshLayerPartitioner.Split(imported.Asset, layerCount, axis);
        Directory.CreateDirectory(outputDirectory);
        var paths = new List<string>(layers.Count);
        foreach (var layer in layers)
        {
            var path = Path.Combine(
                outputDirectory,
                $"{Safe(imported.Asset.Name)}-layer-{layer.Index + 1:D3}.stl"
            );
            BinaryStlWriter.Write(path, layer.Mesh);
            paths.Add(path);
        }
        return new(imported, layers, paths);
    }

    private static string Safe(string value) =>
        string.Concat(value.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
}
