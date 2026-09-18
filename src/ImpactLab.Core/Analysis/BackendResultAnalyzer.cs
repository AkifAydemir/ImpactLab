using ImpactLab.Core.Backends;
using ImpactLab.Core.Continuum.Results;

namespace ImpactLab.Core.Analysis;

public static class BackendResultAnalyzer
{
    public static BackendResultSummary Analyze(string backendId, BackendRunOutput output)
    {
        RunResultSummary compatibility;
        if (output.Domain is LatticeResultDomain lattice)
            compatibility = ResultAnalyzer.Analyze(lattice.Mesh, output.Result);
        else
            compatibility = new(
                output.Result.Frames.LastOrDefault()?.TimeSeconds ?? 0,
                output.Result.PeakKineticEnergyJ,
                output.Result.PeakElasticEnergyJ,
                output.Result.MaxDisplacementMeters,
                output.Result.BrokenSpringCount,
                output.Result.PeakContactForceN,
                output.Result.PeakTangentialForceN,
                output.Result.FrictionEnergyJ,
                []
            );
        var c = output.Native<ContinuumResult>();
        return new(
            backendId,
            output.Domain.Kind.ToString(),
            compatibility,
            c is null ? null : ContinuumResultAnalyzer.Analyze(c)
        );
    }
}
