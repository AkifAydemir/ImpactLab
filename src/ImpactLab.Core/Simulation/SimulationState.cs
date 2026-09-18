using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;

namespace ImpactLab.Core.Simulation;

public sealed class SimulationState
{
    public SimulationState(SimulationMesh mesh)
    {
        Position = new Vec3[mesh.Nodes.Count];
        Velocity = new Vec3[mesh.Nodes.Count];
        Force = new Vec3[mesh.Nodes.Count];
        IsFixed = new bool[mesh.Nodes.Count];
        for (var i = 0; i < mesh.Nodes.Count; i++)
            Position[i] = mesh.Nodes[i].RestPosition;
    }

    public Vec3[] Position { get; }
    public Vec3[] Velocity { get; }
    public Vec3[] Force { get; }
    public bool[] IsFixed { get; }
}
