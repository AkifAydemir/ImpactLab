namespace ImpactLab.Core.Verification;

public sealed record SignalBinding(
    string ReferenceId,
    SignalBindingKind Kind,
    string? SourceId = null,
    double Scale = 1.0,
    double Offset = 0.0
)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ReferenceId))
            throw new InvalidOperationException("Reference id required.");
        if (
            (
                Kind
                is SignalBindingKind.ProbeAverage
                    or SignalBindingKind.ProbeMaximum
                    or SignalBindingKind.ProbeMinimum
                    or SignalBindingKind.ConstraintReactionMagnitude
            ) && string.IsNullOrWhiteSpace(SourceId)
        )
            throw new InvalidOperationException($"Binding {Kind} requires SourceId.");
    }
}
