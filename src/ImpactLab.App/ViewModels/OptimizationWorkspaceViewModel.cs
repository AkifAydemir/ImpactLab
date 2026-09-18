using ImpactLab.Core.Experiments.Optimization;

namespace ImpactLab.App.ViewModels;

public sealed class OptimizationWorkspaceViewModel
{
    public List<OptimizationVariable> Variables { get; } = [];
    public IReadOnlyList<OptimizationEvaluation> Trace { get; private set; } = [];

    public async Task RunAsync(
        Func<
            IReadOnlyDictionary<string, double>,
            CancellationToken,
            Task<(double, bool, string?)>
        > eval,
        int max,
        CancellationToken ct
    )
    {
        Trace = await new NelderMeadOptimizer().OptimizeAsync(Variables, eval, max, ct);
    }
}
