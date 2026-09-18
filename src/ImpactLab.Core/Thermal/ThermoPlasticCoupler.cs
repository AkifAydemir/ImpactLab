using ImpactLab.Core.Continuum.Dynamics;

namespace ImpactLab.Core.Thermal;

public sealed class ThermoPlasticCoupler
{
    public void DepositPlasticHeat(
        ImpactLab.Core.Continuum.TetrahedralMesh mesh,
        ContinuumDynamicState state,
        double[] nodalHeat,
        double dt,
        double fraction = 0.9
    )
    {
        for (var i = 0; i < state.ElementStates.Length; i++)
        {
            var e = mesh.Elements[i];
            var q =
                state.ElementStates[i].DissipatedEnergyJ
                * Math.Clamp(fraction, 0, 1)
                / Math.Max(dt, 1e-30)
                / 4;
            foreach (var n in e.Nodes())
                nodalHeat[n] += q;
            state.ElementStates[i].DissipatedEnergyJ = 0;
        }
    }
}
