using ImpactLab.Core.Continuum.Mechanics;
using ImpactLab.Core.Continuum.Plasticity;

namespace ImpactLab.Core.Continuum.Dynamics;

public sealed class DynamicElementState
{
    public StressTensor6 Stress { get; set; }
    public StrainTensor6 TotalStrain { get; set; }
    public PlasticState Plastic { get; set; } = PlasticState.Zero;
    public double TemperatureKelvin { get; set; } = 293.15;
    public double InternalEnergyJ { get; set; }
    public double DissipatedEnergyJ { get; set; }

    public DynamicElementState Clone() =>
        new()
        {
            Stress = Stress,
            TotalStrain = TotalStrain,
            Plastic = Plastic,
            TemperatureKelvin = TemperatureKelvin,
            InternalEnergyJ = InternalEnergyJ,
            DissipatedEnergyJ = DissipatedEnergyJ,
        };
}
