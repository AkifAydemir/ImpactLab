namespace ImpactLab.Core.Experiments;

public readonly record struct ExperimentProgress(
    int Completed,
    int Total,
    int Running,
    int Failed,
    string Message
)
{
    public double Fraction => Total == 0 ? 0 : (double)Completed / Total;
}
