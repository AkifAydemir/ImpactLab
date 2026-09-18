using System.ComponentModel;
using ImpactLab.Core.Diagnostics;

namespace ImpactLab.App.ViewModels;

public sealed class DiagnosticsWorkspaceViewModel : INotifyPropertyChanged
{
    private IReadOnlyList<PerformanceSample> _samples = [];
    private IReadOnlyList<string> _budgetIssues = [];
    public IReadOnlyList<PerformanceSample> Samples
    {
        get => _samples;
        private set
        {
            _samples = value;
            PropertyChanged?.Invoke(this, new(nameof(Samples)));
        }
    }
    public IReadOnlyList<string> BudgetIssues
    {
        get => _budgetIssues;
        private set
        {
            _budgetIssues = value;
            PropertyChanged?.Invoke(this, new(nameof(BudgetIssues)));
        }
    }

    public void Load(PerformanceTrace trace)
    {
        Samples = trace.Samples;
        BudgetIssues = trace.Evaluate([
            new(PerformanceStage.MeshBuild, TimeSpan.FromSeconds(2)),
            new(PerformanceStage.PostProcessing, TimeSpan.FromSeconds(1)),
        ]);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
