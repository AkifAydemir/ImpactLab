using System.ComponentModel;
using System.Windows.Input;
using ImpactLab.App.Accessibility;
using ImpactLab.App.Infrastructure;
using ImpactLab.App.Layout;
using ImpactLab.App.Services;
using ImpactLab.App.Visualization;
using ImpactLab.Core.Backends;
using ImpactLab.Core.Geometry;
using ImpactLab.Core.IO;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Parameters;
using ImpactLab.Core.Reporting;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.App.ViewModels;

public sealed class ProductShellViewModel : INotifyPropertyChanged
{
    private readonly SimulationRunnerService _simulationRunner;
    private readonly AsyncRelayCommand _runScenarioCommand;
    private WorkspaceTab _selectedTab = WorkspaceTab.Start;
    private string _analysisStatus =
        "New analysis - load a built-in sample or open a scenario file.";
    private string _selectedSampleName;
    private SimulationRunBundle? _lastRun;

    public ProductShellViewModel()
    {
        MaterialsCatalog = new MaterialCatalog();
        Profiles = new MaterialProfileCatalog(MaterialsCatalog);
        BackendService = new BackendCatalogService();
        Workspace = new WorkspaceSessionService();
        ScenarioWorkspace = new ScenarioWorkspaceService(MaterialsCatalog, Profiles);
        Announcements = new AccessibilityAnnouncementService();
        Traversal = new KeyboardTraversalService();
        Traversal.Configure(AccessibilityWorkflowCatalog.Default.Select(x => x.AutomationName));
        LayoutStore = new WorkspaceLayoutStore();
        ExtensionHost = new ExtensionHostService(BackendService.Registry);
        _simulationRunner = new SimulationRunnerService(BackendService.Registry);

        Simulation = new MainViewModel();
        Backend = new BackendViewModel(BackendService);
        Geometry = new GeometryEditorViewModel();
        Assembly = new AssemblyViewModel(MaterialsCatalog);
        Import = new ImportGeometryViewModel(MaterialsCatalog);
        Materials = new MaterialLibraryViewModel(MaterialsCatalog, Profiles);
        Materials.MaterialAdded += (_, _) =>
        {
            Import.RefreshMaterials();
            Assembly.RefreshMaterials();
        };
        Loads = new LoadCaseViewModel();
        Boundaries = new BoundaryConditionViewModel();
        Experiments = new ExperimentViewModel(
            new ExperimentRunnerService(_simulationRunner),
            MaterialsCatalog
        );
        Calibration = new CalibrationViewModel();
        Verification = new VerificationViewModel();
        Mobility = new MobilityViewModel();
        PostProcessing = new PostProcessingViewModel();
        Results = new ResultExplorerViewModel();
        LargeResults = new LargeResultWorkspaceViewModel(Announcements);
        Charts = new ChartWorkspaceViewModel();
        Runs = new RunArchiveViewModel(
            new RunArchiveService(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ImpactLab",
                    "runs"
                )
            )
        );
        Reports = new ReportViewModel();
        Extensions = new ExtensionsWorkspaceViewModel(ExtensionHost);
        Diagnostics = new DiagnosticsWorkspaceViewModel();

        _selectedSampleName = SampleNames[0];
        NewAnalysisCommand = new RelayCommand(NewAnalysis);
        LoadSampleCommand = new RelayCommand(LoadSelectedSample);
        OpenQuickImpactCommand = new RelayCommand(() => SelectedTab = WorkspaceTab.Simulation);
        OpenAssemblyCommand = new RelayCommand(() => SelectedTab = WorkspaceTab.Assembly);
        OpenImportCommand = new RelayCommand(() => SelectedTab = WorkspaceTab.Import);
        _runScenarioCommand = new AsyncRelayCommand(RunScenarioAsync);
        RunScenarioCommand = _runScenarioCommand;
    }

    public MaterialCatalog MaterialsCatalog { get; }
    public MaterialProfileCatalog Profiles { get; }
    public BackendCatalogService BackendService { get; }
    public WorkspaceSessionService Workspace { get; }
    public ScenarioWorkspaceService ScenarioWorkspace { get; }
    public AccessibilityAnnouncementService Announcements { get; }
    public KeyboardTraversalService Traversal { get; }
    public WorkspaceLayoutStore LayoutStore { get; }
    public ExtensionHostService ExtensionHost { get; }
    public MainViewModel Simulation { get; }
    public BackendViewModel Backend { get; }
    public GeometryEditorViewModel Geometry { get; }
    public AssemblyViewModel Assembly { get; }
    public ImportGeometryViewModel Import { get; }
    public MaterialLibraryViewModel Materials { get; }
    public LoadCaseViewModel Loads { get; }
    public BoundaryConditionViewModel Boundaries { get; }
    public ExperimentViewModel Experiments { get; }
    public CalibrationViewModel Calibration { get; }
    public VerificationViewModel Verification { get; }
    public MobilityViewModel Mobility { get; }
    public PostProcessingViewModel PostProcessing { get; }
    public ResultExplorerViewModel Results { get; }
    public LargeResultWorkspaceViewModel LargeResults { get; }
    public ChartWorkspaceViewModel Charts { get; }
    public RunArchiveViewModel Runs { get; }
    public ReportViewModel Reports { get; }
    public ExtensionsWorkspaceViewModel Extensions { get; }
    public DiagnosticsWorkspaceViewModel Diagnostics { get; }

    public IReadOnlyList<WorkspaceTab> Tabs { get; } = Enum.GetValues<WorkspaceTab>();
    public IReadOnlyList<string> SampleNames { get; } =
    ["Plate impact study", "Angle-profile comparison"];
    public ICommand NewAnalysisCommand { get; }
    public ICommand LoadSampleCommand { get; }
    public ICommand OpenQuickImpactCommand { get; }
    public ICommand OpenAssemblyCommand { get; }
    public ICommand OpenImportCommand { get; }
    public ICommand RunScenarioCommand { get; }

    public WorkspaceTab SelectedTab
    {
        get => _selectedTab;
        set
        {
            if (_selectedTab == value)
                return;
            _selectedTab = value;
            Workspace.Document.Layout.SelectedTab = value.ToString();
            Announcements.Announce($"Workspace changed to {value}.");
            Changed(nameof(SelectedTab));
            Changed(nameof(SelectedTabIndex));
        }
    }

    public int SelectedTabIndex
    {
        get => (int)SelectedTab;
        set
        {
            if (Enum.IsDefined(typeof(WorkspaceTab), value))
                SelectedTab = (WorkspaceTab)value;
        }
    }

    public string SelectedSampleName
    {
        get => _selectedSampleName;
        set
        {
            if (_selectedSampleName == value)
                return;
            _selectedSampleName = value;
            Changed(nameof(SelectedSampleName));
        }
    }

    public string AnalysisStatus
    {
        get => _analysisStatus;
        private set
        {
            if (_analysisStatus == value)
                return;
            _analysisStatus = value;
            Changed(nameof(AnalysisStatus));
            Announcements.Announce(value);
        }
    }

    public void NewAnalysis()
    {
        Workspace.New();
        ScenarioWorkspace.New();
        AnalysisStatus =
            "New analysis created. Load a built-in sample or open a scenario file before Solve.";
        SelectedTab = WorkspaceTab.Geometry;
    }

    public void OpenScenario(string path)
    {
        try
        {
            ScenarioWorkspace.Open(path);
            Materials.Refresh();
            Import.RefreshMaterials();
            AnalysisStatus = $"Opened scenario: {ScenarioWorkspace.Current.Name}.";
            SelectedTab = WorkspaceTab.Simulation;
        }
        catch (Exception ex)
        {
            AnalysisStatus = $"Open failed: {ex.Message}";
        }
    }

    public void SaveProject(string path)
    {
        ScenarioWorkspace.Save(path);
        AnalysisStatus = $"Project saved: {Path.GetFileName(path)}.";
    }

    public void ExportResultsCsv(string path)
    {
        var run =
            _lastRun ?? throw new InvalidOperationException("Run a simulation before export.");
        ResultCsvExporter.Write(path, run.Compiled.Mesh, run.Result);
        AnalysisStatus = $"Detailed node results exported: {Path.GetFileName(path)}.";
    }

    public void ExportEngineeringReport(string path)
    {
        var run =
            _lastRun ?? throw new InvalidOperationException("Run a simulation before export.");
        var document = EngineeringReportBuilder.BuildDetailed(
            ScenarioWorkspace.Current,
            run.Backend.DisplayName,
            run.BackendSummary,
            MaterialsCatalog,
            run.Validity,
            Profiles,
            run.EnergyBalance
        );
        new ReportExportService().Export(path, document);
        AnalysisStatus = $"Engineering report exported: {Path.GetFileName(path)}.";
    }

    public void ExportSimulationAnimation(string path)
    {
        var run =
            _lastRun ?? throw new InvalidOperationException("Run a simulation before export.");
        if (string.Equals(Path.GetExtension(path), ".mp4", StringComparison.OrdinalIgnoreCase))
            SimulationAnimationExporter.ExportMp4(path, run);
        else
            SimulationAnimationExporter.ExportGif(path, run);
        AnalysisStatus =
            $"Simulation animation exported: {Path.GetFileName(path)} with timeline metadata.";
    }

    public void UseImportedMeshAsImpactTarget()
    {
        var imported =
            Import.Result
            ?? throw new InvalidOperationException("Import an STL or OBJ mesh first.");
        var selectedMaterial = Import.SelectedTargetMaterial.Id;
        var extent = imported.Asset.Bounds.Max - imported.Asset.Bounds.Min;
        var characteristic = Math.Max(extent.X, Math.Max(extent.Y, extent.Z));
        var cellSize = Math.Clamp(characteristic / 24.0, 0.0005, 0.05);
        var scenario = new ScenarioDefinition
        {
            Name = $"Impact - {imported.Asset.Name}",
            CellSizeMeters = cellSize,
            Meshing = MeshingSettings.FromCellSize(cellSize),
        };
        scenario.Parts.Add(
            new ScenarioPartDefinition
            {
                Id = "imported-target",
                Name = imported.Asset.Name,
                GeometrySource = ScenarioGeometrySourceKind.ImportedMesh,
                ImportedMeshPath = Import.Path,
                ImportedMeshUnit = Import.SourceUnit,
                CenterImportedMesh = Import.CenterAtOrigin,
                WeldImportedMesh = Import.WeldVertices,
                MaterialId = selectedMaterial,
                FixMinimumZPlane = true,
            }
        );
        var impactor = new ScenarioRigidBodyDefinition
        {
            Id = "impactor-1",
            Name = "Rigid impactor",
            GeometryKind = GeometryKind.Sphere,
            MaterialId = Import.SelectedImpactorMaterial.Id,
            InitialVelocity = new Vec3(0, 0, -Math.Abs(Import.ImpactSpeedMps)),
            GeometryParameters = GeometryParameterSchemas.For(GeometryKind.Sphere).CreateDefaults(),
        };
        impactor.GeometryParameters.Set("centerX", imported.Asset.Bounds.Center.X);
        impactor.GeometryParameters.Set("centerY", imported.Asset.Bounds.Center.Y);
        impactor.GeometryParameters.Set(
            "centerZ",
            imported.Asset.Bounds.Max.Z + Math.Max(characteristic * 0.15, cellSize * 2)
        );
        impactor.GeometryParameters.Set(
            "radius",
            Math.Max(Import.ImpactorDiameterMm / 2000.0, cellSize)
        );
        scenario.RigidBodies.Add(impactor);
        scenario.Contacts.Add(new("impactor-1", "imported-target"));
        ScenarioWorkspace.Use(scenario);
        AnalysisStatus =
            $"Imported target is ready: {imported.Asset.Name}, material {selectedMaterial}, "
            + $"impactor {Import.SelectedImpactorMaterial.DisplayName} at "
            + $"{Math.Abs(Import.ImpactSpeedMps):G4} m/s, cell size {cellSize * 1000:G4} mm. "
            + "Review setup and select Solve.";
        SelectedTab = WorkspaceTab.Simulation;
    }

    public void UseAssemblyAsScenario()
    {
        var scenario = Assembly.BuildScenario();
        ScenarioWorkspace.Use(scenario);
        AnalysisStatus =
            $"Assembly is ready: {scenario.Parts.Count} target/layer parts, "
            + $"{scenario.RigidBodies.Count} impactors and {scenario.Contacts.Count} contact pairs. "
            + "Review the backend and stability checks before solving.";
        SelectedTab = WorkspaceTab.Simulation;
    }

    private void LoadSelectedSample()
    {
        ScenarioDefinition scenario = SelectedSampleName switch
        {
            "Angle-profile comparison" => ScenarioPresetLibrary.CreateAngleProfileImpact(),
            _ => ScenarioPresetLibrary.CreatePlateImpact(),
        };
        ScenarioWorkspace.Use(scenario);
        AnalysisStatus =
            $"Loaded built-in sample: {scenario.Name}. Run Solve to generate fresh results.";
        SelectedTab = WorkspaceTab.Simulation;
    }

    private async Task RunScenarioAsync()
    {
        if (ScenarioWorkspace.Current.Parts.Count == 0)
        {
            AnalysisStatus =
                "Solve not started: load a built-in sample or open a scenario containing at least one part.";
            return;
        }
        if (Backend.Selected is null)
        {
            AnalysisStatus = "Solve not started: select a backend.";
            return;
        }

        try
        {
            var scenario = ScenarioWorkspace.Current;
            var backend = Backend.Selected;
            AnalysisStatus = $"Running {scenario.Name} with {backend.DisplayName}...";
            var run = await Task.Run(() =>
                _simulationRunner.Run(scenario, MaterialsCatalog, backendId: backend.Id)
            );
            _lastRun = run;
            Results.Load(run);
            PostProcessing.Load(run);
            Verification.LoadRunValidity(run.Validity);
            AnalysisStatus =
                $"Run completed: {scenario.Name} / {backend.DisplayName}. "
                + $"Validity screening: {run.Validity.PassedCount} passed, "
                + $"{run.Validity.WarningCount} warnings, {run.Validity.FailedCount} failed. "
                + "Inspect Results and the exported report before interpreting the run.";
            SelectedTab = WorkspaceTab.Results;
        }
        catch (Exception ex)
        {
            AnalysisStatus = $"Solve failed: {ex.Message}";
        }
        finally
        {
            _runScenarioCommand.RaiseCanExecuteChanged();
        }
    }

    private void Changed(string propertyName) => PropertyChanged?.Invoke(this, new(propertyName));

    public event PropertyChangedEventHandler? PropertyChanged;
}
