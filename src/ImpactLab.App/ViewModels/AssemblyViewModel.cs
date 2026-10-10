using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ImpactLab.Core.Geometry;
using ImpactLab.Core.Geometry.Imported;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Scenarios;
using ImpactLab.Core.Scene;

namespace ImpactLab.App.ViewModels;

public sealed class AssemblyViewModel : INotifyPropertyChanged
{
    private readonly MaterialCatalog _materials;
    private readonly MeshImportService _importer = new();
    private AssemblyPartViewModel? _selectedPart;
    private string _status = "Add two or more OBJ/STL files to build an impact assembly.";

    public AssemblyViewModel(MaterialCatalog materials) => _materials = materials;

    public ObservableCollection<AssemblyPartViewModel> Parts { get; } = [];
    public IReadOnlyList<MaterialDefinition> Materials =>
        _materials.Entries.Select(x => x.Definition).ToArray();
    public IReadOnlyList<ScenarioPartRole> Roles { get; } = Enum.GetValues<ScenarioPartRole>();
    public IReadOnlyList<LengthUnit> LengthUnits { get; } = Enum.GetValues<LengthUnit>();
    public string AssemblyName { get; set; } = "Imported impact assembly";
    public string AssemblyId { get; set; } = "main-assembly";
    public double CellSizeMm { get; set; } = 5;
    public double DurationMs { get; set; } = 3;
    public double TimeStepMicroseconds { get; set; } = 0.5;

    public AssemblyPartViewModel? SelectedPart
    {
        get => _selectedPart;
        set => SetField(ref _selectedPart, value);
    }

    public string Status
    {
        get => _status;
        private set => SetField(ref _status, value);
    }

    public void AddFiles(IEnumerable<string> paths)
    {
        var added = 0;
        foreach (var path in paths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var sourceUnit = GuessUnit(path);
            var result = _importer.Import(
                path,
                new MeshImportOptions(sourceUnit, CenterAtOrigin: true, WeldVertices: true)
            );
            var role = Parts.Any(x => x.Role == ScenarioPartRole.Target)
                ? ScenarioPartRole.Target
                : ScenarioPartRole.Target;
            var row = new AssemblyPartViewModel
            {
                Name = System.IO.Path.GetFileNameWithoutExtension(path),
                Path = path,
                SourceUnit = sourceUnit,
                Role = role,
                Material = Materials.First(x => x.Id == "steel-s355jr"),
                LayerIndex = Parts.Count,
                TriangleCount = result.Asset.TriangleCount,
                IsLikelyClosed = result.Inspection.IsLikelyClosed,
            };
            Parts.Add(row);
            SelectedPart = row;
            added++;
        }
        Status =
            $"Assembly contains {Parts.Count} parts; added {added}. "
            + "Parts remain separate so contacts, materials and results can be traced independently.";
        Changed(nameof(Parts));
    }

    public void RemoveSelected()
    {
        if (SelectedPart is null)
            return;
        Parts.Remove(SelectedPart);
        SelectedPart = Parts.LastOrDefault();
        Status = $"Assembly contains {Parts.Count} parts.";
    }

    public void MarkSelectedAsImpactor()
    {
        if (SelectedPart is null)
            return;
        SelectedPart.Role = ScenarioPartRole.Impactor;
        if (SelectedPart.VelocityZ == 0)
            SelectedPart.VelocityZ = -30;
        Status = $"{SelectedPart.Name} is the moving impactor.";
    }

    public ScenarioDefinition BuildScenario()
    {
        if (Parts.Count == 0)
            throw new InvalidOperationException("Add at least one target mesh.");
        if (Parts.All(x => x.Role == ScenarioPartRole.Impactor))
            throw new InvalidOperationException(
                "An assembly needs at least one deformable target part."
            );

        var cellSize = Math.Max(CellSizeMm / 1000.0, 1e-5);
        var scenario = new ScenarioDefinition
        {
            Name = string.IsNullOrWhiteSpace(AssemblyName)
                ? "Imported impact assembly"
                : AssemblyName.Trim(),
            CellSizeMeters = cellSize,
            Meshing = MeshingSettings.FromCellSize(cellSize),
        };
        scenario.Settings = scenario.Settings with
        {
            DurationSeconds = Math.Max(DurationMs / 1000.0, 1e-7),
            TimeStepSeconds = Math.Max(TimeStepMicroseconds / 1_000_000.0, 1e-9),
        };

        var deformableIds = new List<string>();
        foreach (var row in Parts.OrderBy(x => x.LayerIndex))
        {
            var id = SafeId(row.Name, row.Id);
            var transform = row.Transform();
            if (row.Role == ScenarioPartRole.Impactor)
            {
                scenario.RigidBodies.Add(
                    new ScenarioRigidBodyDefinition
                    {
                        Id = id,
                        Name = row.Name,
                        GeometrySource = ScenarioGeometrySourceKind.ImportedMesh,
                        ImportedMeshPath = row.Path,
                        ImportedMeshUnit = row.SourceUnit,
                        CenterImportedMesh = true,
                        WeldImportedMesh = true,
                        MaterialId = row.Material.Id,
                        InitialVelocity = new(row.VelocityX, row.VelocityY, row.VelocityZ),
                        InitialAngularVelocity = new(
                            row.AngularVelocityX,
                            row.AngularVelocityY,
                            row.AngularVelocityZ
                        ),
                        Transform = transform,
                        AssemblyId = AssemblyId,
                    }
                );
                continue;
            }

            scenario.Parts.Add(
                new ScenarioPartDefinition
                {
                    Id = id,
                    Name = row.Name,
                    GeometrySource = ScenarioGeometrySourceKind.ImportedMesh,
                    ImportedMeshPath = row.Path,
                    ImportedMeshUnit = row.SourceUnit,
                    CenterImportedMesh = true,
                    WeldImportedMesh = true,
                    MaterialId = row.Material.Id,
                    Behavior = ScenePartBehavior.Deformable,
                    FixMinimumZPlane =
                        row.Role is ScenarioPartRole.Fixture or ScenarioPartRole.Environment,
                    Transform = transform,
                    AssemblyId = AssemblyId,
                    Role = row.Role,
                    LayerIndex = row.LayerIndex,
                    NominalLayerThicknessMeters =
                        row.LayerThicknessMm > 0 ? row.LayerThicknessMm / 1000.0 : null,
                }
            );
            deformableIds.Add(id);
        }

        foreach (var rigid in scenario.RigidBodies)
        foreach (var targetId in deformableIds)
            scenario.Contacts.Add(new(rigid.Id, targetId));

        Status =
            $"Scenario built with {scenario.Parts.Count} target/layer parts, "
            + $"{scenario.RigidBodies.Count} imported impactors and {scenario.Contacts.Count} contact pairs.";
        return scenario;
    }

    public void RefreshMaterials()
    {
        Changed(nameof(Materials));
    }

    private static LengthUnit GuessUnit(string path) =>
        System.IO.Path.GetExtension(path).Equals(".stl", StringComparison.OrdinalIgnoreCase)
            ? LengthUnit.Millimeter
            : LengthUnit.Meter;

    private static string SafeId(string name, string fallback)
    {
        var clean = new string(
            name.ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray()
        ).Trim('-');
        return $"{(clean.Length == 0 ? "part" : clean)}-{fallback[..8]}";
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        Changed(name!);
        return true;
    }

    private void Changed(string name) => PropertyChanged?.Invoke(this, new(name));

    public event PropertyChangedEventHandler? PropertyChanged;
}

public sealed class AssemblyPartViewModel : INotifyPropertyChanged
{
    private string _name = "Part";
    private ScenarioPartRole _role;
    private MaterialDefinition _material = MaterialLibrary.All[0];

    public string Id { get; } = Guid.NewGuid().ToString("N");
    public string Path { get; init; } = "";
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }
    public ScenarioPartRole Role
    {
        get => _role;
        set => SetField(ref _role, value);
    }
    public MaterialDefinition Material
    {
        get => _material;
        set => SetField(ref _material, value);
    }
    public LengthUnit SourceUnit { get; set; } = LengthUnit.Millimeter;
    public int LayerIndex { get; set; }
    public double LayerThicknessMm { get; set; }
    public double TranslationXmm { get; set; }
    public double TranslationYmm { get; set; }
    public double TranslationZmm { get; set; }
    public double RotationXDeg { get; set; }
    public double RotationYDeg { get; set; }
    public double RotationZDeg { get; set; }
    public double ScaleX { get; set; } = 1;
    public double ScaleY { get; set; } = 1;
    public double ScaleZ { get; set; } = 1;
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double VelocityZ { get; set; }
    public double AngularVelocityX { get; set; }
    public double AngularVelocityY { get; set; }
    public double AngularVelocityZ { get; set; }
    public int TriangleCount { get; init; }
    public bool IsLikelyClosed { get; init; }

    public GeometryTransform3 Transform()
    {
        const double degrees = Math.PI / 180.0;
        var qx = QuaternionD.FromAxisAngle(Vec3.UnitX, RotationXDeg * degrees);
        var qy = QuaternionD.FromAxisAngle(Vec3.UnitY, RotationYDeg * degrees);
        var qz = QuaternionD.FromAxisAngle(Vec3.UnitZ, RotationZDeg * degrees);
        return new(
            new(TranslationXmm / 1000.0, TranslationYmm / 1000.0, TranslationZmm / 1000.0),
            (qz * qy * qx).Normalized(),
            new(Math.Max(ScaleX, 1e-6), Math.Max(ScaleY, 1e-6), Math.Max(ScaleZ, 1e-6))
        );
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;
        field = value;
        PropertyChanged?.Invoke(this, new(name));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
