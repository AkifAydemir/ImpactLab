using ImpactLab.Core.Mathematics;

namespace ImpactLab.App.Authoring;

public sealed class MultiSelectionTransformService
{
    public IReadOnlyDictionary<string, RigidTransform> Translate(
        IReadOnlyDictionary<string, RigidTransform> current,
        IEnumerable<string> ids,
        Vec3 delta
    )
    {
        var set = ids.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return current.ToDictionary(
            x => x.Key,
            x =>
                set.Contains(x.Key)
                    ? x.Value with
                    {
                        Translation = x.Value.Translation + delta,
                    }
                    : x.Value,
            StringComparer.OrdinalIgnoreCase
        );
    }
}
