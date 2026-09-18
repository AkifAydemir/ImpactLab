using ImpactLab.Core.Scenarios;

namespace ImpactLab.Core.Experiments;

public static class ScenarioParameterBinder
{
    public static void Apply(
        ScenarioDefinition scenario,
        IEnumerable<ScenarioParameterBinding> bindings
    )
    {
        foreach (var b in bindings)
            ScenarioValuePath.Apply(scenario, b.Path, b.Value);
    }

    public static IReadOnlyList<ScenarioParameterBinding> From(SweepPoint point) =>
        point.Values.Select(x => new ScenarioParameterBinding(x.Key, x.Value)).ToArray();
}
