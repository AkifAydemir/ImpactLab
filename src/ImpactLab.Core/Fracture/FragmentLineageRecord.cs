namespace ImpactLab.Core.Fracture;

public sealed record FragmentLineageRecord(
    string FragmentId,
    string PartId,
    string? ParentFragmentId,
    int Generation,
    int BirthStep,
    double BirthTimeSeconds,
    IReadOnlyList<int> BirthNodeIds
);
