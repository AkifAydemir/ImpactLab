using ImpactLab.Core.Analysis;

namespace ImpactLab.App.ViewModels;

public sealed class RunSummaryViewModel
{
    public void Load(RunResultSummary summary)
    {
        EndTime = summary.EndTimeSeconds;
        PeakKineticEnergy = summary.PeakKineticEnergyJ;
        PeakElasticEnergy = summary.PeakElasticEnergyJ;
        MaxDisplacement = summary.MaxDisplacementMeters;
        BrokenSprings = summary.BrokenSpringCount;
        PeakContactForce = summary.PeakContactForceN;
        PeakTangentialForce = summary.PeakTangentialForceN;
        FrictionEnergy = summary.FrictionEnergyJ;
        Parts = summary.Parts;
    }

    public double EndTime { get; private set; }
    public double PeakKineticEnergy { get; private set; }
    public double PeakElasticEnergy { get; private set; }
    public double MaxDisplacement { get; private set; }
    public int BrokenSprings { get; private set; }
    public double PeakContactForce { get; private set; }
    public double PeakTangentialForce { get; private set; }
    public double FrictionEnergy { get; private set; }
    public IReadOnlyList<PartResultSummary> Parts { get; private set; } = [];
}
