namespace ImpactLab.Core.Finalization;

public sealed record ReleaseReadinessReport(
    DateTimeOffset GeneratedUtc,
    IReadOnlyList<ReleaseGateResult> Gates
)
{
    public IReadOnlyList<ReleaseGateKind> MissingGates =>
        Enum.GetValues<ReleaseGateKind>().Where(kind => Gates.All(x => x.Gate != kind)).ToArray();

    public IReadOnlyList<ReleaseGateKind> DuplicateGates =>
        Gates
            .GroupBy(x => x.Gate)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(kind => (int)kind)
            .ToArray();

    public bool HasCompleteGateSet =>
        MissingGates.Count == 0
        && DuplicateGates.Count == 0
        && Gates.Count == Enum.GetValues<ReleaseGateKind>().Length;

    public bool Ready => HasCompleteGateSet && Gates.All(x => x.Passed);

    public IReadOnlyList<ReleaseGateResult> Blocking => Gates.Where(x => x.Blocking).ToArray();

    public IReadOnlyList<ReleaseGateResult> EnvironmentBlocked =>
        Gates.Where(x => x.Status == ReleaseGateStatus.EnvironmentBlocked).ToArray();

    public IReadOnlyList<ReleaseGateResult> Failed =>
        Gates.Where(x => x.Status == ReleaseGateStatus.Failed).ToArray();
}
