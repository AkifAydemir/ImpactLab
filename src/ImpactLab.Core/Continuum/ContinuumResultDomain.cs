using ImpactLab.Core.Backends;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum;

public sealed class ContinuumResultDomain : IResultDomain
{
    public ContinuumResultDomain(TetrahedralMesh mesh)
    {
        Mesh = mesh;
        RestPositions = mesh.Nodes.Select(x => x.Position).ToArray();
        PartIds = mesh.Nodes.Select(x => x.PartId).ToArray();
    }

    public TetrahedralMesh Mesh { get; }
    public ResultDomainKind Kind => ResultDomainKind.TetrahedralContinuum;
    public int NodeCount => Mesh.Nodes.Count;
    public IReadOnlyList<Vec3> RestPositions { get; }
    public IReadOnlyList<string> PartIds { get; }
}
