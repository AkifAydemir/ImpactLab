namespace ImpactLab.Core.Thermal;

public sealed class ThermalState
{
    public ThermalState(int nodeCount, double initialKelvin = 293.15)
    {
        TemperatureKelvin = Enumerable.Repeat(initialKelvin, nodeCount).ToArray();
    }

    public double[] TemperatureKelvin { get; }
}
