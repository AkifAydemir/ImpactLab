namespace ImpactLab.Core.Projects;

public sealed record ProjectFileDto
{
    public int FormatVersion { get; init; } = 1;
    public string Name { get; init; } = "Untitled";
    public List<ProjectPartDto> Parts { get; init; } = [];
    public List<ProjectRigidBodyDto> RigidBodies { get; init; } = [];
    public ProjectSimulationSettingsDto Settings { get; init; } = new();
}

public sealed record ProjectPartDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Behavior { get; init; } = "Deformable";
    public string MaterialId { get; init; } = "generic-steel";
    public ProjectGeometryDto Geometry { get; init; } = new();
}

public sealed record ProjectRigidBodyDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string MaterialId { get; init; } = "generic-steel";
    public ProjectGeometryDto Geometry { get; init; } = new();
    public double[] Velocity { get; init; } = [0, 0, 0];
    public double[] AngularVelocity { get; init; } = [0, 0, 0];
    public double? MassOverrideKg { get; init; }
}

public sealed record ProjectGeometryDto
{
    public string Kind { get; init; } = "Box";
    public string Name { get; init; } = "Geometry";
    public double[] Center { get; init; } = [0, 0, 0];
    public Dictionary<string, double> Parameters { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);
}

public sealed record ProjectSimulationSettingsDto
{
    public double TimeStepSeconds { get; init; } = 0.000002;
    public double DurationSeconds { get; init; } = 0.001;
    public int SnapshotStride { get; init; } = 10;
    public int TelemetryStride { get; init; } = 10;
}
