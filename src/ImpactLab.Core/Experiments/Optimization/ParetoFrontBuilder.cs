namespace ImpactLab.Core.Experiments.Optimization;

public static class ParetoFrontBuilder
{
    public static IReadOnlyList<MultiObjectiveValue> Build(
        IReadOnlyList<MultiObjectiveValue> points
    ) =>
        points
            .Where(
                (p, i) => !points.Where((_, j) => j != i).Any(q => ParetoDominance.Dominates(q, p))
            )
            .ToArray();
}
