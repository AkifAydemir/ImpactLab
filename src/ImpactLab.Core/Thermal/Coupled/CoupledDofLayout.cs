namespace ImpactLab.Core.Thermal.Coupled;

public sealed record CoupledDofLayout(int NodeCount)
{
    public int MechanicalDofs => NodeCount * 3;
    public int ThermalDofs => NodeCount;
    public int TotalDofs => NodeCount * 4;

    public int UX(int n) => n * 4;

    public int UY(int n) => n * 4 + 1;

    public int UZ(int n) => n * 4 + 2;

    public int T(int n) => n * 4 + 3;
}
