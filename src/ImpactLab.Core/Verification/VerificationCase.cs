using ImpactLab.Core.Scenarios;

namespace ImpactLab.Core.Verification;

public sealed class VerificationCase
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Verification case";
    public ScenarioDefinition Scenario { get; set; } = new();
    public List<ReferenceSignal> References { get; } = [];
    public List<SignalBinding> Bindings { get; } = [];
    public double RmseTolerance { get; set; } = double.PositiveInfinity;
    public double PeakRelativeTolerance { get; set; } = double.PositiveInfinity;

    public void Validate()
    {
        foreach (var r in References)
            r.Validate();
        foreach (var b in Bindings)
            b.Validate();
        var ids = References.Select(x => x.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (Bindings.Any(x => !ids.Contains(x.ReferenceId)))
            throw new InvalidOperationException(
                "Every signal binding must reference a known signal."
            );
    }
}
