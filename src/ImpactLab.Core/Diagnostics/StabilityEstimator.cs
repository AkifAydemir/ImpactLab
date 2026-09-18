using ImpactLab.Core.Meshing;

namespace ImpactLab.Core.Diagnostics;

public static class StabilityEstimator
{
    public static StabilityEstimate Estimate(SimulationMesh mesh, double requestedDt)
    {
        var critical = double.PositiveInfinity;
        foreach (var spring in mesh.Springs)
        {
            var a = mesh.Nodes[spring.NodeA];
            var b = mesh.Nodes[spring.NodeB];
            var stiffness =
                spring.Material.YoungModulusPa * spring.EffectiveAreaM2 / spring.RestLength;
            var reducedMass = (a.MassKg * b.MassKg) / Math.Max(a.MassKg + b.MassKg, 1e-18);
            var omega = Math.Sqrt(Math.Max(stiffness / Math.Max(reducedMass, 1e-18), 0.0));
            if (omega > 1e-12)
                critical = Math.Min(critical, 2.0 / omega);
        }
        if (double.IsPositiveInfinity(critical))
            critical = requestedDt;
        var safe = critical * 0.75;
        var ratio = requestedDt / Math.Max(safe, 1e-18);
        return new StabilityEstimate(
            requestedDt,
            safe,
            ratio,
            ratio <= 1.0,
            ratio <= 1.0
                ? "Requested explicit time step is within the conservative estimate."
                : "Requested time step exceeds the conservative explicit stability estimate."
        );
    }
}
