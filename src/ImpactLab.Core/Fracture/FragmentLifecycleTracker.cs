using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Fracture;

public sealed class FragmentLifecycleTracker
{
    private readonly Dictionary<string, FragmentCoarseBodyState> _states = new(
        StringComparer.Ordinal
    );

    public IReadOnlyCollection<FragmentCoarseBodyState> States => _states.Values;

    public IReadOnlyList<FragmentCoarseBodyState> SpawnNew(
        IEnumerable<FragmentCoarseState> projectedStates
    )
    {
        var spawned = new List<FragmentCoarseBodyState>();
        foreach (var projection in projectedStates)
        {
            if (_states.ContainsKey(projection.Id))
                continue;
            var state = new FragmentCoarseBodyState(projection);
            _states.Add(state.Id, state);
            spawned.Add(state);
        }
        return spawned;
    }

    public bool TryGet(string fragmentId, out FragmentCoarseBodyState? state) =>
        _states.TryGetValue(fragmentId, out state);

    public void IntegrateAll(
        double timeStepSeconds,
        in Vec3 gravity,
        double linearDamping = 0.0,
        double angularDamping = 0.0
    )
    {
        foreach (var state in _states.Values)
            state.Integrate(timeStepSeconds, gravity, linearDamping, angularDamping);
    }
}
