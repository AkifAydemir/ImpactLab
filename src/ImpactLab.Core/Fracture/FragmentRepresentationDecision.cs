namespace ImpactLab.Core.Fracture;

public sealed record FragmentRepresentationDecision(
    string FragmentId,
    FragmentRepresentationKind Kind,
    string Reason
);
