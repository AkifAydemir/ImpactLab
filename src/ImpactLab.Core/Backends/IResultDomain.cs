using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Backends;

public interface IResultDomain
{
    ResultDomainKind Kind { get; }
    int NodeCount { get; }
    IReadOnlyList<Vec3> RestPositions { get; }
    IReadOnlyList<string> PartIds { get; }
}
