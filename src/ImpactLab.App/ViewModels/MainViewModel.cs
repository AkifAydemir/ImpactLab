using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using ImpactLab.App.Infrastructure;
using ImpactLab.App.Rendering;
using ImpactLab.Core.Analysis;
using ImpactLab.Core.Constraints;
using ImpactLab.Core.Contact;
using ImpactLab.Core.Geometry;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Physics;
using ImpactLab.Core.Rigid;
using ImpactLab.Core.Scene;
using ImpactLab.Core.Simulation;

namespace ImpactLab.App.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly RelayCommand _runCommand;
    private readonly RelayCommand _previousFrameCommand;
    private readonly RelayCommand _nextFrameCommand;
    private bool _isRunning;
    private string _status = "Ready";
    private GeometryKind _selectedTargetGeometry = GeometryKind.Plate;
    private GeometryKind _selectedImpactorGeometry = GeometryKind.Sphere;
    private MaterialDefinition _selectedTargetMaterial = MaterialLibrary.All[0];
    private MaterialDefinition _selectedFrontLayerMaterial = MaterialLibrary.All[2];
    private MaterialDefinition _selectedBackingMaterial = MaterialLibrary.All[0];
    private MaterialDefinition _selectedImpactorMaterial = MaterialLibrary.All[0];
    private HeatmapMode _selectedHeatmap = HeatmapMode.Damage;
    private Model3DGroup _scene = new();
    private double _maxDisplacementMm;
    private int _brokenSpringCount;
    private int _peakContactCount;
    private double _peakContactForceKn;
    private double _computeMilliseconds;
    private int _telemetrySampleCount;
    private int _currentFrameIndex;
    private int _frameCount;
    private double _frameTimeMs;
    private SimulationMesh? _lastMesh;
    private SimulationResult? _lastResult;
    private IReadOnlyList<RigidBodyDefinition> _lastRigidBodies = [];

    public MainViewModel()
    {
        _runCommand = new RelayCommand(() => _ = RunAsync(), () => !IsRunning);
        _previousFrameCommand = new RelayCommand(
            PreviousFrame,
            () => _lastResult is not null && CurrentFrameIndex > 0
        );
        _nextFrameCommand = new RelayCommand(
            NextFrame,
            () => _lastResult is not null && CurrentFrameIndex + 1 < FrameCount
        );
        RunCommand = _runCommand;
        PreviousFrameCommand = _previousFrameCommand;
        NextFrameCommand = _nextFrameCommand;
        TargetGeometryKinds = Enum.GetValues<GeometryKind>();
        ImpactorGeometryKinds = [GeometryKind.Sphere, GeometryKind.Box, GeometryKind.Cylinder];
        Materials = MaterialLibrary.All;
        HeatmapModes = Enum.GetValues<HeatmapMode>();
    }

    public IReadOnlyList<GeometryKind> TargetGeometryKinds { get; }
    public IReadOnlyList<GeometryKind> ImpactorGeometryKinds { get; }
    public IReadOnlyList<MaterialDefinition> Materials { get; }
    public IReadOnlyList<HeatmapMode> HeatmapModes { get; }
    public ICommand RunCommand { get; }
    public ICommand PreviousFrameCommand { get; }
    public ICommand NextFrameCommand { get; }
    public GeometryKind SelectedTargetGeometry
    {
        get => _selectedTargetGeometry;
        set => SetField(ref _selectedTargetGeometry, value);
    }
    public GeometryKind SelectedImpactorGeometry
    {
        get => _selectedImpactorGeometry;
        set => SetField(ref _selectedImpactorGeometry, value);
    }
    public MaterialDefinition SelectedTargetMaterial
    {
        get => _selectedTargetMaterial;
        set => SetField(ref _selectedTargetMaterial, value);
    }
    public MaterialDefinition SelectedFrontLayerMaterial
    {
        get => _selectedFrontLayerMaterial;
        set => SetField(ref _selectedFrontLayerMaterial, value);
    }
    public MaterialDefinition SelectedBackingMaterial
    {
        get => _selectedBackingMaterial;
        set => SetField(ref _selectedBackingMaterial, value);
    }
    public MaterialDefinition SelectedImpactorMaterial
    {
        get => _selectedImpactorMaterial;
        set => SetField(ref _selectedImpactorMaterial, value);
    }
    public HeatmapMode SelectedHeatmap
    {
        get => _selectedHeatmap;
        set
        {
            if (SetField(ref _selectedHeatmap, value))
                RenderCurrentFrame();
        }
    }
    public bool UseFrontLayer { get; set; } = true;
    public bool UseBackingPart { get; set; } = true;
    public double FrontLayerThicknessMm { get; set; } = 15.0;
    public double BackingThicknessMm { get; set; } = 30.0;
    public double BackingGapMm { get; set; } = 5.0;
    public double InterfaceStrengthPercent { get; set; } = 75.0;
    public double CellSizeMm { get; set; } = 10.0;
    public double ImpactorDiameterMm { get; set; } = 50.0;
    public double ImpactorLengthMm { get; set; } = 70.0;
    public double ImpactSpeedMps { get; set; } = 30.0;
    public double SimulationDurationMs { get; set; } = 3.0;
    public double TimeStepMicroseconds { get; set; } = 2.0;
    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (SetField(ref _isRunning, value))
                _runCommand.RaiseCanExecuteChanged();
        }
    }
    public string Status
    {
        get => _status;
        private set => SetField(ref _status, value);
    }
    public Model3DGroup Scene
    {
        get => _scene;
        private set => SetField(ref _scene, value);
    }
    public double MaxDisplacementMm
    {
        get => _maxDisplacementMm;
        private set => SetField(ref _maxDisplacementMm, value);
    }
    public int BrokenSpringCount
    {
        get => _brokenSpringCount;
        private set => SetField(ref _brokenSpringCount, value);
    }
    public int PeakContactCount
    {
        get => _peakContactCount;
        private set => SetField(ref _peakContactCount, value);
    }
    public double PeakContactForceKn
    {
        get => _peakContactForceKn;
        private set => SetField(ref _peakContactForceKn, value);
    }
    public double ComputeMilliseconds
    {
        get => _computeMilliseconds;
        private set => SetField(ref _computeMilliseconds, value);
    }
    public int TelemetrySampleCount
    {
        get => _telemetrySampleCount;
        private set => SetField(ref _telemetrySampleCount, value);
    }
    public int CurrentFrameIndex
    {
        get => _currentFrameIndex;
        private set
        {
            if (SetField(ref _currentFrameIndex, value))
            {
                _previousFrameCommand.RaiseCanExecuteChanged();
                _nextFrameCommand.RaiseCanExecuteChanged();
            }
        }
    }
    public int FrameCount
    {
        get => _frameCount;
        private set
        {
            if (SetField(ref _frameCount, value))
            {
                _previousFrameCommand.RaiseCanExecuteChanged();
                _nextFrameCommand.RaiseCanExecuteChanged();
            }
        }
    }
    public double FrameTimeMs
    {
        get => _frameTimeMs;
        private set => SetField(ref _frameTimeMs, value);
    }
    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task RunAsync()
    {
        try
        {
            IsRunning = true;
            Status = "Compiling multi-part scene and material interfaces...";
            var targetGeometry = BuildTargetGeometry(SelectedTargetGeometry);
            var cellSize = CellSizeMm / 1000.0;
            var scene = BuildScene(targetGeometry);
            var interfaceTable = BuildInterfaceTable();
            var compiled = SceneMeshCompiler.Compile(scene, cellSize, interfaceTable);
            var mesh = compiled.Mesh;
            var impactorGeometry = BuildImpactorGeometry(
                SelectedImpactorGeometry,
                targetGeometry.Bounds.Max.Z
            );
            var impactor = new RigidBodyDefinition(
                "impactor-1",
                "Rigid impact body",
                impactorGeometry,
                SelectedImpactorMaterial,
                new Vec3(0.0, 0.0, -Math.Abs(ImpactSpeedMps))
            );
            var settings = new SimulationSettings
            {
                TimeStepSeconds = TimeStepMicroseconds / 1_000_000.0,
                DurationSeconds = SimulationDurationMs / 1000.0,
                SnapshotStride = 10,
                TelemetryStride = 5,
                Gravity = Vec3.Zero,
                GlobalVelocityDamping = 0.002,
            };
            var constraints = BuildConstraints(scene, cellSize);
            var contactInteractions = BuildContactInteractions(scene, impactor);
            var solver = new ExplicitLatticeSolver(new MaterialResponseDamageModel());
            var result = await Task.Run(() =>
                solver.Run(
                    mesh,
                    settings,
                    [],
                    constraints,
                    [impactor],
                    ContactSettings.Default,
                    contactInteractions
                )
            );
            _lastMesh = mesh;
            _lastResult = result;
            _lastRigidBodies = [impactor];
            FrameCount = result.Frames.Count;
            CurrentFrameIndex = Math.Max(0, FrameCount - 1);
            RenderCurrentFrame();
            MaxDisplacementMm = result.MaxDisplacementMeters * 1000.0;
            BrokenSpringCount = result.BrokenSpringCount;
            PeakContactCount = result.PeakContactCount;
            PeakContactForceKn = result.PeakContactForceN / 1000.0;
            ComputeMilliseconds = result.ComputeTime.TotalMilliseconds;
            TelemetrySampleCount = result.Telemetry.Samples.Count;
            Status =
                $"Complete - {mesh.Parts.Count} parts / "
                + $"{mesh.Nodes.Count:N0} nodes / {mesh.Springs.Count:N0} links";
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
        }
        finally
        {
            IsRunning = false;
        }
    }

    private SimulationScene BuildScene(GeometrySpec targetGeometry)
    {
        var scene = new SimulationScene();
        scene.Add(
            new ScenePart(
                "target-main",
                "Primary target",
                targetGeometry,
                SelectedTargetMaterial,
                ScenePartBehavior.Deformable,
                BuildMaterialRegions(targetGeometry)
            )
        );
        if (UseBackingPart)
        {
            var bounds = targetGeometry.Bounds;
            var thickness = Math.Max(0.002, BackingThicknessMm / 1000.0);
            var gap = Math.Max(0.0, BackingGapMm / 1000.0);
            var center = new Vec3(
                bounds.Center.X,
                bounds.Center.Y,
                bounds.Min.Z - gap - thickness * 0.5
            );
            var size = new Vec3(
                Math.Max(bounds.Size.X, CellSizeMm / 1000.0),
                Math.Max(bounds.Size.Y, CellSizeMm / 1000.0),
                thickness
            );
            scene.Add(
                new ScenePart(
                    "backing-part",
                    "Secondary backing part",
                    new BoxSpec("Backing part", center, size),
                    SelectedBackingMaterial,
                    ScenePartBehavior.Deformable
                )
            );
        }
        return scene;
    }

    private MaterialInterfaceTable BuildInterfaceTable()
    {
        var strength = Math.Clamp(InterfaceStrengthPercent / 100.0, 0.05, 2.0);
        return new MaterialInterfaceTable(
            new MaterialInterfaceDefinition(
                "ui-default-interface",
                "User interface bond",
                strength,
                1.0,
                strength,
                strength
            )
        );
    }

    private static IReadOnlyList<IFixedConstraint> BuildConstraints(
        SimulationScene scene,
        double cellSize
    )
    {
        var constraints = new List<IFixedConstraint>();
        foreach (var part in scene.Parts.Where(x => x.Behavior != ScenePartBehavior.Rigid))
        {
            constraints.Add(
                new FixedPlaneConstraint(
                    ConstraintAxis.Z,
                    ConstraintSide.Minimum,
                    cellSize * 0.55,
                    part.Id
                )
            );
        }
        return constraints;
    }

    private static ContactInteractionTable BuildContactInteractions(
        SimulationScene scene,
        RigidBodyDefinition impactor
    )
    {
        var table = new ContactInteractionTable();
        foreach (var part in scene.Parts.Where(x => x.Behavior != ScenePartBehavior.Rigid))
        {
            table.Set(new ContactPairRule(impactor.Id, part.Id, Enabled: true));
        }
        return table;
    }

    private IReadOnlyList<MaterialRegion> BuildMaterialRegions(GeometrySpec target)
    {
        if (!UseFrontLayer)
            return [];
        var thickness = Math.Clamp(FrontLayerThicknessMm / 1000.0, 0.001, target.Bounds.Size.Z);
        var bounds = target.Bounds;
        var center = new Vec3(bounds.Center.X, bounds.Center.Y, bounds.Max.Z - thickness * 0.5);
        var size = new Vec3(bounds.Size.X + 0.002, bounds.Size.Y + 0.002, thickness);
        return
        [
            new MaterialRegion(
                "front-layer",
                new BoxSpec("Front material region", center, size),
                SelectedFrontLayerMaterial,
                Priority: 100
            ),
        ];
    }

    private void PreviousFrame()
    {
        if (CurrentFrameIndex <= 0)
            return;
        CurrentFrameIndex--;
        RenderCurrentFrame();
    }

    private void NextFrame()
    {
        if (_lastResult is null || CurrentFrameIndex + 1 >= _lastResult.Frames.Count)
            return;
        CurrentFrameIndex++;
        RenderCurrentFrame();
    }

    private void RenderCurrentFrame()
    {
        if (_lastMesh is null || _lastResult is null || _lastResult.Frames.Count == 0)
            return;
        var index = Math.Clamp(CurrentFrameIndex, 0, _lastResult.Frames.Count - 1);
        var frame = _lastResult.Frames[index];
        var visual = MeshSceneBuilder.Build(_lastMesh, frame, SelectedHeatmap);
        for (var i = 0; i < frame.RigidBodies.Count && i < _lastRigidBodies.Count; i++)
        {
            visual.Children.Add(
                RigidBodySceneBuilder.Build(_lastRigidBodies[i], frame.RigidBodies[i])
            );
        }
        Scene = visual;
        FrameTimeMs = frame.TimeSeconds * 1000.0;
    }

    private static GeometrySpec BuildTargetGeometry(GeometryKind kind) =>
        kind switch
        {
            GeometryKind.Plate => new RectangularPlateSpec(
                "Plate",
                Vec3.Zero,
                width: 0.30,
                height: 0.30,
                thickness: 0.05
            ),
            GeometryKind.Box => new BoxSpec("Box", Vec3.Zero, new Vec3(0.24, 0.24, 0.12)),
            GeometryKind.Cylinder => new CylinderSpec(
                "Cylinder",
                Vec3.Zero,
                radius: 0.12,
                length: 0.20
            ),
            GeometryKind.Tube => new TubeSpec(
                "Tube",
                Vec3.Zero,
                outerRadius: 0.12,
                wallThickness: 0.035,
                length: 0.20
            ),
            GeometryKind.AngleProfile => new AngleProfileSpec(
                "Angle profile",
                Vec3.Zero,
                legX: 0.22,
                legY: 0.22,
                thickness: 0.05,
                length: 0.22
            ),
            GeometryKind.Sphere => new SphereSpec("Sphere target", Vec3.Zero, radius: 0.12),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };

    private GeometrySpec BuildImpactorGeometry(GeometryKind kind, double targetTopZ)
    {
        var radius = Math.Max(0.002, ImpactorDiameterMm / 2000.0);
        var length = Math.Max(0.004, ImpactorLengthMm / 1000.0);
        var gap = Math.Max(CellSizeMm / 1000.0, 0.005);
        var halfHeight = kind switch
        {
            GeometryKind.Sphere => radius,
            GeometryKind.Box => length * 0.5,
            GeometryKind.Cylinder => length * 0.5,
            _ => radius,
        };
        var center = new Vec3(0.0, 0.0, targetTopZ + halfHeight + gap);
        return kind switch
        {
            GeometryKind.Sphere => new SphereSpec("Rigid sphere", center, radius),
            GeometryKind.Box => new BoxSpec(
                "Rigid box",
                center,
                new Vec3(radius * 2.0, radius * 2.0, length)
            ),
            GeometryKind.Cylinder => new CylinderSpec("Rigid cylinder", center, radius, length),
            _ => throw new NotSupportedException(
                $"{kind} is not enabled as a moving rigid geometry in v3."
            ),
        };
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
