namespace ImpactLab.Core.Physics;

public sealed record PhysicsModelDescriptor(
    string Id,
    string DisplayName,
    Version Version,
    PhysicsModelCategory Category,
    PhysicsModelValidationLevel ValidationLevel,
    string Description,
    IReadOnlyList<string>? Dependencies = null
)
{
    public IReadOnlyList<string> RequiredModels => Dependencies ?? [];

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id) || string.IsNullOrWhiteSpace(DisplayName))
            throw new InvalidOperationException("Physics model id and display name are required.");
        if (Version.Major < 0)
            throw new InvalidOperationException("Physics model version is invalid.");
        if (RequiredModels.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException("Physics model dependency ids cannot be empty.");
        if (RequiredModels.Contains(Id, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("A physics model cannot depend on itself.");
    }
}
