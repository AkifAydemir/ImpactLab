namespace ImpactLab.Core.Sparse;

public interface IPreconditioner
{
    string Id { get; }
    void Apply(ReadOnlySpan<double> input, Span<double> output);
}
