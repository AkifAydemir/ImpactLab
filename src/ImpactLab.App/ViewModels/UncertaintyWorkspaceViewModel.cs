using ImpactLab.Core.Experiments.Uncertainty;

namespace ImpactLab.App.ViewModels;

public sealed class UncertaintyWorkspaceViewModel
{
    public int Samples { get; set; } = 50;
    public int Seed { get; set; } = 12345;
    public List<ParameterDistribution> Parameters { get; } = [];

    public IReadOnlyList<IReadOnlyDictionary<string, double>> Preview() =>
        new LatinHypercubeSampler().Generate(Parameters, Samples, Seed);
}
