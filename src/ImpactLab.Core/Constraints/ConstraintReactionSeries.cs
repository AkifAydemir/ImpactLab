namespace ImpactLab.Core.Constraints;

public sealed class ConstraintReactionSeries
{
    private readonly Dictionary<string, List<ConstraintReactionSample>> _byConstraint = new(
        StringComparer.OrdinalIgnoreCase
    );
    public IReadOnlyDictionary<string, IReadOnlyList<ConstraintReactionSample>> ByConstraint =>
        _byConstraint.ToDictionary(
            x => x.Key,
            x => (IReadOnlyList<ConstraintReactionSample>)x.Value
        );
    public IEnumerable<ConstraintReactionSample> AllSamples =>
        _byConstraint.Values.SelectMany(x => x).OrderBy(x => x.TimeSeconds);

    public void Add(ConstraintReactionSample sample)
    {
        if (!_byConstraint.TryGetValue(sample.ConstraintId, out var list))
            _byConstraint[sample.ConstraintId] = list = [];
        list.Add(sample);
    }

    public double PeakMagnitude(string constraintId) =>
        _byConstraint.TryGetValue(constraintId, out var list) && list.Count > 0
            ? list.Max(x => x.MagnitudeN)
            : 0.0;
}
