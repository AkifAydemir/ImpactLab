namespace ImpactLab.Core.PostProcessing;

public readonly record struct ProbeSample(
    double TimeSeconds,
    double Minimum,
    double Maximum,
    double Average,
    double Rms,
    int SampleCount
)
{
    public double Value => Average;
}
