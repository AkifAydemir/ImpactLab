using ImpactLab.Core.Materials;

namespace ImpactLab.Core.Backends;

public sealed class AdvancedSpringRuntime
{
    public AdvancedSpringRuntime(int count)
    {
        PlasticStrain = new double[count];
        TemperatureKelvin = new double[count];
        CohesiveStates = new CohesiveInterfaceState?[count];
        for (var i = 0; i < count; i++)
            TemperatureKelvin[i] = 293.15;
    }

    public double[] PlasticStrain { get; }
    public double[] TemperatureKelvin { get; }
    public CohesiveInterfaceState?[] CohesiveStates { get; }
}
