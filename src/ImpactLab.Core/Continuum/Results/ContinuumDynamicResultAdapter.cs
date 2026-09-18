using ImpactLab.Core.Constraints;
using ImpactLab.Core.Continuum.Dynamics;
using ImpactLab.Core.Continuum.Results;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Continuum.Results;

public static class ContinuumDynamicResultAdapter
{
    public static ContinuumResult ToContinuum(ContinuumDynamicResult d)
    {
        var frames = d
            .Frames.Select(f => new ContinuumFrame(
                f.TimeSeconds,
                f.Position,
                f.Displacement,
                f.TemperatureKelvin,
                new ContinuumElementFrame(
                    f.ElementStress,
                    f.ElementStrain,
                    f.ElementStress.Select(x => x.VonMises).ToArray()
                )
            ))
            .ToArray();
        return new(d.Mesh, frames, [], d.ComputeTime);
    }

    public static SimulationResult ToCompatibility(ContinuumDynamicResult d)
    {
        var c = ToContinuum(d);
        return ContinuumResultAdapter.ToCompatibility(c);
    }
}
