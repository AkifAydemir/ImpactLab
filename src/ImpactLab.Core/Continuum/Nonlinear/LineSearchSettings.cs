namespace ImpactLab.Core.Continuum.Nonlinear;

public sealed record LineSearchSettings(
    bool Enabled = true,
    double InitialScale = 1.0,
    double MinimumScale = 0.05,
    double Reduction = 0.5,
    int MaxTrials = 8
)
{
    public void Validate()
    {
        if (
            InitialScale <= 0
            || MinimumScale <= 0
            || MinimumScale > InitialScale
            || Reduction <= 0
            || Reduction >= 1
            || MaxTrials < 1
        )
            throw new InvalidOperationException("Invalid line-search settings.");
    }
}
