namespace ImpactLab.Core.Experiments.Sensitivity;

public sealed record SaltelliSamplingPlan(
    int BaseSamples,
    int ParameterCount,
    int Seed = 12345,
    bool IncludeSecondOrder = false
)
{
    public int EvaluationCount => BaseSamples * (2 + ParameterCount * (IncludeSecondOrder ? 2 : 1));
}
