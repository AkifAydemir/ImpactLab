using ImpactLab.Core.Constraints;

namespace ImpactLab.Core.Simulation;

public sealed record SimulationResult(
    IReadOnlyList<SimulationFrame> Frames,
    SimulationTelemetrySeries Telemetry,
    double MaxDisplacementMeters,
    int BrokenSpringCount,
    double PeakKineticEnergyJ,
    double PeakElasticEnergyJ,
    int PeakContactCount,
    double MaxContactPenetrationMeters,
    double PeakContactForceN,
    double PeakTangentialForceN,
    double FrictionEnergyJ,
    TimeSpan ComputeTime,
    ConstraintReactionSeries ConstraintReactions
);
