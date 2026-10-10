using System.ComponentModel;
using ImpactLab.Core.Geometry.Imported;
using ImpactLab.Core.Materials;

namespace ImpactLab.App.ViewModels;

public sealed class ImportGeometryViewModel : INotifyPropertyChanged
{
    private readonly MeshImportService _service = new();
    private readonly MaterialCatalog _materials;
    private MeshImportResult? _result;
    private string _path = "";
    private string _status = "Choose an OBJ or STL file.";
    private IReadOnlyList<string> _layerPaths = [];

    public ImportGeometryViewModel(MaterialCatalog materials)
    {
        _materials = materials;
        SelectedTargetMaterial = Materials.First(x => x.Id == "steel-s355jr");
        SelectedImpactorMaterial = Materials.First(x => x.Id == "steel-42crmo4-qt");
    }

    public string Path
    {
        get => _path;
        set
        {
            _path = value;
            PropertyChanged?.Invoke(this, new(nameof(Path)));
        }
    }
    public MeshImportResult? Result
    {
        get => _result;
        private set
        {
            _result = value;
            PropertyChanged?.Invoke(this, new(nameof(Result)));
        }
    }
    public LengthUnit SourceUnit { get; set; } = LengthUnit.Millimeter;
    public bool CenterAtOrigin { get; set; } = true;
    public bool WeldVertices { get; set; } = true;
    public int LayerCount { get; set; } = 4;
    public MeshLayerAxis LayerAxis { get; set; } = MeshLayerAxis.Z;
    public IReadOnlyList<MeshLayerAxis> LayerAxes { get; } = Enum.GetValues<MeshLayerAxis>();
    public IReadOnlyList<LengthUnit> LengthUnits { get; } = Enum.GetValues<LengthUnit>();
    public IReadOnlyList<MaterialDefinition> Materials =>
        _materials.Entries.Select(x => x.Definition).ToArray();
    public MaterialDefinition SelectedTargetMaterial { get; set; }
    public MaterialDefinition SelectedImpactorMaterial { get; set; }
    public double ImpactSpeedMps { get; set; } = 30;
    public double ImpactorDiameterMm { get; set; } = 50;
    public IReadOnlyList<string> LayerPaths
    {
        get => _layerPaths;
        private set
        {
            _layerPaths = value;
            PropertyChanged?.Invoke(this, new(nameof(LayerPaths)));
        }
    }
    public string Status
    {
        get => _status;
        private set
        {
            _status = value;
            PropertyChanged?.Invoke(this, new(nameof(Status)));
        }
    }

    public void Import()
    {
        Result = _service.Import(Path, Options());
        Status =
            $"Imported {Result.Asset.TriangleCount:N0} triangles; "
            + $"likely closed={Result.Inspection.IsLikelyClosed}, "
            + $"boundary edges={Result.Inspection.BoundaryEdges}.";
    }

    public void ExportLayers(string outputDirectory)
    {
        var exported = new StlLayerExportService().Export(
            Path,
            outputDirectory,
            LayerCount,
            LayerAxis,
            Options()
        );
        Result = exported.Import;
        LayerPaths = exported.Paths;
        Status =
            $"Exported {exported.Paths.Count} non-empty surface layers to {outputDirectory}. "
            + "Triangles crossing a layer plane are assigned by centroid; exported layers are intended for segmentation/authoring.";
    }

    public void RefreshMaterials() => PropertyChanged?.Invoke(this, new(nameof(Materials)));

    private MeshImportOptions Options() => new(SourceUnit, CenterAtOrigin, WeldVertices, 1e-8);

    public event PropertyChangedEventHandler? PropertyChanged;
}
