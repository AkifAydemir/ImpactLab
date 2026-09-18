namespace ImpactLab.Core.Extensions.Security;

public static class PermissionEvaluator
{
    public static PermissionDecision Evaluate(
        ExtensionPermission requested,
        ExtensionPermission maximum
    )
    {
        var granted = requested & maximum;
        var denied = Enum.GetValues<ExtensionPermission>()
            .Where(x =>
                x != ExtensionPermission.None
                && x != ExtensionPermission.All
                && requested.HasFlag(x)
                && !granted.HasFlag(x)
            )
            .ToArray();
        return new(requested, granted, denied);
    }
}
