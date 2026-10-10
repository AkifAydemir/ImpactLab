using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Fracture;

public static class FragmentStateTransfer
{
    public static IReadOnlyList<FragmentCoarseState> ProjectCoarseStates(
        IReadOnlyList<FragmentClusterSnapshot> fragments,
        IReadOnlyList<FragmentRepresentationDecision> decisions
    )
    {
        var decisionsById = decisions.ToDictionary(x => x.FragmentId, StringComparer.Ordinal);
        if (decisionsById.Count != decisions.Count)
            throw new InvalidOperationException(
                "Fragment representation decisions must be unique."
            );

        var projected = new List<FragmentCoarseState>();
        foreach (var fragment in fragments)
        {
            if (!decisionsById.TryGetValue(fragment.Id, out var decision))
                throw new InvalidOperationException(
                    $"Fragment '{fragment.Id}' has no representation decision."
                );
            if (decision.Kind == FragmentRepresentationKind.ResolvedDeformable)
                continue;
            projected.Add(
                new(
                    fragment.Id,
                    fragment.PartId,
                    decision.Kind,
                    fragment.NodeIds.ToArray(),
                    fragment.ContactSamples.ToArray(),
                    fragment.MassKg,
                    fragment.CenterOfMass,
                    fragment.LinearVelocity,
                    fragment.InertiaTensorKgM2,
                    fragment.InertiaConditionNumber,
                    fragment.AngularMomentumKgM2PerSec,
                    fragment.AngularVelocityRadPerSec,
                    QuaternionD.Identity,
                    fragment.Bounds,
                    fragment.TranslationalKineticEnergyJ,
                    fragment.RotationalKineticEnergyJ,
                    fragment.UnresolvedInternalKineticEnergyJ
                )
            );
        }

        var knownIds = fragments.Select(x => x.Id).ToHashSet(StringComparer.Ordinal);
        var orphan = decisions.FirstOrDefault(x => !knownIds.Contains(x.FragmentId));
        if (orphan is not null)
            throw new InvalidOperationException(
                $"Representation decision '{orphan.FragmentId}' has no fragment state."
            );
        return projected;
    }

    public static FragmentTransferAudit AuditCoarseProjection(
        IReadOnlyList<FragmentClusterSnapshot> fragments,
        IReadOnlyList<FragmentCoarseState> projected
    )
    {
        var sourceById = fragments.ToDictionary(x => x.Id, StringComparer.Ordinal);
        var selectedSources = new List<FragmentClusterSnapshot>(projected.Count);
        foreach (var state in projected)
        {
            if (!sourceById.TryGetValue(state.Id, out var source))
                throw new InvalidOperationException(
                    $"Projected fragment '{state.Id}' has no source fragment."
                );
            selectedSources.Add(source);
        }

        var sourceMass = selectedSources.Sum(x => x.MassKg);
        var projectedMass = projected.Sum(x => x.MassKg);
        var sourceLinearMomentum = selectedSources.Aggregate(
            Vec3.Zero,
            (sum, x) => sum + x.LinearMomentumKgMPerSec
        );
        var projectedLinearMomentum = projected.Aggregate(
            Vec3.Zero,
            (sum, x) => sum + x.LinearMomentumKgMPerSec
        );
        var sourceAngularMomentum = selectedSources.Aggregate(
            Vec3.Zero,
            (sum, x) => sum + x.AngularMomentumKgM2PerSec
        );
        var projectedAngularMomentum = projected.Aggregate(
            Vec3.Zero,
            (sum, x) => sum + x.AngularMomentumKgM2PerSec
        );
        var sourceEnergy = selectedSources.Sum(x => x.TotalNodalKineticEnergyJ);
        var projectedEnergy = projected.Sum(x => x.AccountedKineticEnergyJ);
        return new(
            projected.Count,
            sourceMass,
            projectedMass,
            projectedMass - sourceMass,
            sourceLinearMomentum,
            projectedLinearMomentum,
            (projectedLinearMomentum - sourceLinearMomentum).Length,
            sourceAngularMomentum,
            projectedAngularMomentum,
            (projectedAngularMomentum - sourceAngularMomentum).Length,
            sourceEnergy,
            projectedEnergy,
            projectedEnergy - sourceEnergy
        );
    }
}
