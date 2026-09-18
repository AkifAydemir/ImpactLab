namespace ImpactLab.Core.Continuum.Mechanics;

public sealed class ContinuumDofMap
{
    public ContinuumDofMap(int nodeCount)
    {
        NodeCount = nodeCount;
    }

    public int NodeCount { get; }
    public int DofCount => NodeCount * 3;

    public int X(int n) => n * 3;

    public int Y(int n) => n * 3 + 1;

    public int Z(int n) => n * 3 + 2;

    public int[] ElementDofs(TetraElement e) =>
        [
            X(e.A),
            Y(e.A),
            Z(e.A),
            X(e.B),
            Y(e.B),
            Z(e.B),
            X(e.C),
            Y(e.C),
            Z(e.C),
            X(e.D),
            Y(e.D),
            Z(e.D),
        ];
}
