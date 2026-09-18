namespace ImpactLab.Core.Calibration;

public static class CalibrationEvaluator
{
    public static double Score(
        CalibrationObservation observation,
        IReadOnlyList<CalibrationObjective> objectives
    )
    {
        if (objectives.Count == 0)
            throw new InvalidOperationException("No calibration objectives configured.");
        return objectives.Sum(x => x.Evaluate(observation));
    }
}
