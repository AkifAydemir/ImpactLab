using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Fields;

public enum ScalarFieldCombineMode
{
    Add,
    Multiply,
    Maximum,
    Minimum,
}

public sealed class CompositeScalarField : IScalarField3
{
    public CompositeScalarField(IEnumerable<IScalarField3> fields, ScalarFieldCombineMode mode)
    {
        Fields = fields.ToArray();
        if (Fields.Count == 0)
            throw new ArgumentException("Composite field requires children.", nameof(fields));
        Mode = mode;
    }

    public IReadOnlyList<IScalarField3> Fields { get; }
    public ScalarFieldCombineMode Mode { get; }

    public double Sample(in Vec3 position, double timeSeconds)
    {
        var samplePosition = position;
        var values = Fields.Select(x => x.Sample(samplePosition, timeSeconds)).ToArray();
        return Mode switch
        {
            ScalarFieldCombineMode.Add => values.Sum(),
            ScalarFieldCombineMode.Multiply => values.Aggregate(1.0, (a, b) => a * b),
            ScalarFieldCombineMode.Maximum => values.Max(),
            ScalarFieldCombineMode.Minimum => values.Min(),
            _ => 0.0,
        };
    }
}
