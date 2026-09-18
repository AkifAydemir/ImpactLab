using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.PostProcessing;

public static class ContactHistoryAnalyzer
{
    public static ContactHistorySeries Extract(
        SimulationResult result,
        string rigidBodyId,
        string targetPartId
    )
    {
        var samples = new List<ContactHistorySample>();
        foreach (var frame in result.Frames)
        {
            var m = frame.ContactPairs.FirstOrDefault(x =>
                string.Equals(x.RigidBodyId, rigidBodyId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.TargetPartId, targetPartId, StringComparison.OrdinalIgnoreCase)
            );
            samples.Add(
                new ContactHistorySample(
                    frame.TimeSeconds,
                    m.ContactCount,
                    m.MaxPenetrationMeters,
                    m.TotalNormalForceN,
                    m.TotalTangentialForceN,
                    m.DissipatedFrictionEnergyJ
                )
            );
        }
        return new(rigidBodyId, targetPartId, samples);
    }
}
