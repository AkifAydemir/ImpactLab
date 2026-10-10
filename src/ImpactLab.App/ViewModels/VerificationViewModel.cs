using System.ComponentModel;
using ImpactLab.Core.Verification;
using ImpactLab.Core.Verification.Benchmarks;

namespace ImpactLab.App.ViewModels;

public sealed class VerificationViewModel : INotifyPropertyChanged
{
    private VerificationSuite _suite = new();
    private IReadOnlyList<VerificationResult> _results = [];
    private VerificationBenchmarkSuiteResult? _benchmarks;
    private RunValidityAssessment? _runValidity;
    public VerificationSuite Suite => _suite;
    public IReadOnlyList<VerificationResult> Results
    {
        get => _results;
        private set
        {
            _results = value;
            Changed(nameof(Results));
        }
    }
    public VerificationBenchmarkSuiteResult? Benchmarks
    {
        get => _benchmarks;
        private set
        {
            _benchmarks = value;
            Changed(nameof(Benchmarks));
            Changed(nameof(BenchmarkSummary));
        }
    }
    public string BenchmarkSummary =>
        Benchmarks is null
            ? "Not executed"
            : $"{Benchmarks.Benchmarks.Count(x => x.Passed)}/{Benchmarks.Benchmarks.Count} source benchmarks pass";
    public RunValidityAssessment? RunValidity
    {
        get => _runValidity;
        private set
        {
            _runValidity = value;
            Changed(nameof(RunValidity));
            Changed(nameof(RunValidityChecks));
            Changed(nameof(RunValiditySummary));
        }
    }
    public IReadOnlyList<RunValidityCheck> RunValidityChecks => RunValidity?.Checks ?? [];
    public string RunValiditySummary =>
        RunValidity is null
            ? "Run a simulation to execute validity screening."
            : $"{RunValidity.PassedCount} passed, {RunValidity.WarningCount} warnings, {RunValidity.FailedCount} failed";

    public void SetResults(IReadOnlyList<VerificationResult> results) => Results = results;

    public void RunSourceBenchmarks() => Benchmarks = VerificationBenchmarkRunner.Run();

    public void LoadRunValidity(RunValidityAssessment assessment) => RunValidity = assessment;

    private void Changed(string name) => PropertyChanged?.Invoke(this, new(name));

    public event PropertyChangedEventHandler? PropertyChanged;
}
