namespace ImpactLab.Core.Fracture;

public sealed class FragmentRepresentationPolicy
{
    private readonly FragmentRepresentationSettings _settings;

    public FragmentRepresentationPolicy(FragmentRepresentationSettings? settings = null)
    {
        _settings = settings ?? new();
        _settings.Validate();
    }

    public FragmentRepresentationDecision Decide(FragmentClusterSnapshot fragment)
    {
        if (fragment.ContainsConstrainedNode)
            return new(
                fragment.Id,
                FragmentRepresentationKind.ResolvedDeformable,
                "Support-connected clusters remain resolved."
            );
        if (
            fragment.NodeIds.Count >= _settings.MinimumResolvedNodeCount
            && fragment.MassKg >= _settings.MinimumResolvedMassKg
        )
            return new(
                fragment.Id,
                FragmentRepresentationKind.ResolvedDeformable,
                "Mass and node count exceed the resolved-fragment thresholds."
            );
        if (
            fragment.NodeIds.Count >= _settings.MinimumReducedOrderNodeCount
            && fragment.MassKg >= _settings.MinimumReducedOrderMassKg
        )
            return new(
                fragment.Id,
                FragmentRepresentationKind.ReducedOrderRigid,
                "Fragment retains rigid-body mass and inertia without full internal resolution."
            );
        return new(
            fragment.Id,
            FragmentRepresentationKind.StatisticalParticle,
            "Fragment is below the configured reduced-order thresholds."
        );
    }

    public IReadOnlyList<FragmentRepresentationDecision> DecideAll(
        IReadOnlyList<FragmentClusterSnapshot> fragments
    )
    {
        var detachedIds = FragmentClusterAnalyzer
            .Detached(fragments)
            .Select(x => x.Id)
            .ToHashSet(StringComparer.Ordinal);
        return fragments
            .Select(fragment =>
                detachedIds.Contains(fragment.Id)
                    ? Decide(fragment)
                    : new FragmentRepresentationDecision(
                        fragment.Id,
                        FragmentRepresentationKind.ResolvedDeformable,
                        "The support-connected or primary cluster remains resolved."
                    )
            )
            .ToArray();
    }
}
