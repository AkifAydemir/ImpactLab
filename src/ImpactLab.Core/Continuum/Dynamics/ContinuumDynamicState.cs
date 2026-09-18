using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Dynamics;

public sealed class ContinuumDynamicState
{
    public ContinuumDynamicState(int nodeCount, int elementCount)
    {
        Displacement = new Vec3[nodeCount];
        Velocity = new Vec3[nodeCount];
        Acceleration = new Vec3[nodeCount];
        ElementStates = Enumerable
            .Range(0, elementCount)
            .Select(_ => new DynamicElementState())
            .ToArray();
    }

    public Vec3[] Displacement { get; }
    public Vec3[] Velocity { get; }
    public Vec3[] Acceleration { get; }
    public DynamicElementState[] ElementStates { get; }
    public double TimeSeconds { get; set; }
}
