using System.ComponentModel;
using ImpactLab.Core.Verification;
using ImpactLab.Core.Verification.Benchmarks;

namespace ImpactLab.App.ViewModels;

public sealed class VerificationViewModel : INotifyPropertyChanged
{
    private VerificationSuite _suite = new();
    private IReadOnlyList<VerificationResult> _results = [];
    private VerificationBenchmarkSuiteResult? _benchmarks;
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

    public void SetResults(IReadOnlyList<VerificationResult> results) => Results = results;

    public void RunSourceBenchmarks() => Benchmarks = VerificationBenchmarkRunner.Run();

    private void Changed(string name) => PropertyChanged?.Invoke(this, new(name));

    public event PropertyChangedEventHandler? PropertyChanged;
}
