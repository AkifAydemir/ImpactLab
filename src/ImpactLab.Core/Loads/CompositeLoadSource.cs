using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Loads;

public sealed class CompositeLoadSource : ILoadSource
{
    public CompositeLoadSource(IEnumerable<ILoadSource> children) => Children = children.ToArray();

    public IReadOnlyList<ILoadSource> Children { get; }

    public void Apply(
        double timeSeconds,
        double timeStepSeconds,
        SimulationMesh mesh,
        SimulationState state
    )
    {
        foreach (var child in Children)
            child.Apply(timeSeconds, timeStepSeconds, mesh, state);
    }
}
