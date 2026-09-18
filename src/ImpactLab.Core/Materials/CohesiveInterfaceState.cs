namespace ImpactLab.Core.Materials;

public sealed class CohesiveInterfaceState
{
    public double Damage { get; set; }
    public double MaximumEquivalentSeparationMeters { get; set; }
    public double DissipatedEnergyJ { get; set; }
    public bool Failed => Damage >= 0.999999;
}
