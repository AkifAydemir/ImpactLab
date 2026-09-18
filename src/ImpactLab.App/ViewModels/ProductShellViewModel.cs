using System.ComponentModel;
using System.Windows.Input;
using ImpactLab.App.Accessibility;
using ImpactLab.App.Infrastructure;
using ImpactLab.App.Layout;
using ImpactLab.App.Services;
using ImpactLab.App.Visualization;
using ImpactLab.Core.Backends;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.App.ViewModels;

public sealed class ProductShellViewModel : INotifyPropertyChanged
{
    private readonly SimulationRunnerService _simulationRunner;
    private readonly AsyncRelayCommand _runScenarioCommand;
    private WorkspaceTab _selectedTab = WorkspaceTab.Simulation;
    private string _analysisStatus =
        "New analysis - load a built-in sample or open a scenario file.";
    private string _selectedSampleName;

    public ProductShellViewModel()
    {
        MaterialsCatalog = new MaterialCatalog();
        Profiles = new MaterialProfileCatalog(MaterialsCatalog);
        BackendService = new BackendCatalogService();
        Workspace = new WorkspaceSessionService();
        ScenarioWorkspace = new ScenarioWorkspaceService();
        Announcements = new AccessibilityAnnouncementService();
        Traversal = new KeyboardTraversalService();
        Traversal.Configure(AccessibilityWorkflowCatalog.Default.Select(x => x.AutomationName));
        LayoutStore = new WorkspaceLayoutStore();
        ExtensionHost = new ExtensionHostService(BackendService.Registry);
        _simulationRunner = new SimulationRunnerService(BackendService.Registry);

        Simulation = new MainViewModel();
        Backend = new BackendViewModel(BackendService);
        Geometry = new GeometryEditorViewModel();
        Import = new ImportGeometryViewModel();
        Materials = new MaterialLibraryViewModel(MaterialsCatalog);
        Loads = new LoadCaseViewModel();
        Boundaries = new BoundaryConditionViewModel();
        Experiments = new ExperimentViewModel(
            new ExperimentRunnerService(_simulationRunner),
            MaterialsCatalog
        );
        Calibration = new CalibrationViewModel();
        Verification = new VerificationViewModel();
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
    public ImportGeometryViewModel Import { get; }
    public MaterialLibraryViewModel Materials { get; }
    public LoadCaseViewModel Loads { get; }
    public BoundaryConditionViewModel Boundaries { get; }
    public ExperimentViewModel Experiments { get; }
    public CalibrationViewModel Calibration { get; }
    public VerificationViewModel Verification { get; }
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
            AnalysisStatus = $"Opened scenario: {ScenarioWorkspace.Current.Name}.";
            SelectedTab = WorkspaceTab.Simulation;
        }
        catch (Exception ex)
        {
            AnalysisStatus = $"Open failed: {ex.Message}";
        }
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
            Results.Load(run);
            PostProcessing.Load(run);
            AnalysisStatus =
                $"Run completed: {scenario.Name} / {backend.DisplayName}. Inspect Results and Post-processing; numerical validation is not implied.";
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
