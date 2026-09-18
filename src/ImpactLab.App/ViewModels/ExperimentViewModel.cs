using System.ComponentModel;
using ImpactLab.App.Infrastructure;
using ImpactLab.App.Services;
using ImpactLab.Core.Experiments;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.App.ViewModels;

public sealed class ExperimentViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly ExperimentRunnerService _runner;
    private readonly MaterialCatalog _materials;
    private CancellationTokenSource? _cts;
    private string _status = "Ready";
    private double _progress;

    public ExperimentViewModel(ExperimentRunnerService runner, MaterialCatalog materials)
    {
        _runner = runner;
        _materials = materials;
        Definition = new ExperimentDefinition
        {
            Name = "Thickness and speed study",
            BaseScenario = ScenarioPresetLibrary.CreatePlateImpact(),
            Sweep = new SweepDefinition { Name = "Thickness x speed" },
        };
        Definition.Sweep.Axes.Add(
            new SweepAxis("part:target-main:thickness", [0.03, 0.04, 0.05, 0.06, 0.08])
        );
        Definition.Sweep.Axes.Add(new SweepAxis("rigid:impactor-1:speedz", [-20, -30, -40]));
        RunCommand = new AsyncRelayCommand(RunAsync, () => _cts is null);
    }

    public ExperimentDefinition Definition { get; }
    public ExperimentResult? Result { get; private set; }
    public AsyncRelayCommand RunCommand { get; }
    public string Status
    {
        get => _status;
        private set
        {
            _status = value;
            PropertyChanged?.Invoke(this, new(nameof(Status)));
        }
    }
    public double Progress
    {
        get => _progress;
        private set
        {
            _progress = value;
            PropertyChanged?.Invoke(this, new(nameof(Progress)));
        }
    }

    private async Task RunAsync()
    {
        _cts = new CancellationTokenSource();
        RunCommand.RaiseCanExecuteChanged();
        Status = "Running experiment...";
        try
        {
            var progress = new Progress<(int Done, int Total, string Label)>(x =>
            {
                Progress = x.Total == 0 ? 0 : (double)x.Done / x.Total;
                Status = x.Label;
            });
            Result = await _runner.RunAsync(Definition, _materials, progress, _cts.Token);
            Status = $"Complete: {Result.SuccessCount}/{Result.Runs.Count} successful";
            PropertyChanged?.Invoke(this, new(nameof(Result)));
        }
        finally
        {
            _cts.Dispose();
            _cts = null;
            RunCommand.RaiseCanExecuteChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }
}
