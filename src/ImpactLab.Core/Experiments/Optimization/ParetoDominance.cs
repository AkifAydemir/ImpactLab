namespace ImpactLab.Core.Experiments.Optimization;

public static class ParetoDominance
{
    public static bool Dominates(MultiObjectiveValue a, MultiObjectiveValue b)
    {
        var feasibleA = a.Constraints.All(x => x <= 0);
        var feasibleB = b.Constraints.All(x => x <= 0);
        if (feasibleA && !feasibleB)
            return true;
        if (!feasibleA)
            return false;
        var noWorse = true;
        var better = false;
        for (var i = 0; i < Math.Min(a.Objectives.Count, b.Objectives.Count); i++)
        {
            noWorse &= a.Objectives[i] <= b.Objectives[i];
            better |= a.Objectives[i] < b.Objectives[i];
        }
        return noWorse && better;
    }
}
