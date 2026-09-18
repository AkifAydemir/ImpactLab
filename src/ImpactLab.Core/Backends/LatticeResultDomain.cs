using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;

namespace ImpactLab.Core.Backends;

public sealed class LatticeResultDomain : IResultDomain
{
    public LatticeResultDomain(SimulationMesh mesh)
    {
        Mesh = mesh;
        RestPositions = mesh.Nodes.Select(x => x.RestPosition).ToArray();
        PartIds = mesh.Nodes.Select(x => x.PartId).ToArray();
    }

    public SimulationMesh Mesh { get; }
    public ResultDomainKind Kind => ResultDomainKind.Lattice;
    public int NodeCount => Mesh.Nodes.Count;
    public IReadOnlyList<Vec3> RestPositions { get; }
    public IReadOnlyList<string> PartIds { get; }
}
