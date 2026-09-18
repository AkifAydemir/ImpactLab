namespace ImpactLab.Core.Thermal;

public sealed record ThermalEnergyLedger(
    double InternalEnergyJ,
    double ConductedEnergyJ,
    double ConvectedEnergyJ,
    double RadiatedEnergyJ,
    double InelasticHeatJ,
    double ExternalHeatJ
)
{
    public double ResidualJ =>
        ExternalHeatJ + InelasticHeatJ - InternalEnergyJ - ConvectedEnergyJ - RadiatedEnergyJ;
}
