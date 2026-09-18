namespace ImpactLab.Core.IO;

public readonly record struct SchemaVersion(int Major, int Minor)
{
    public override string ToString() => $"{Major}.{Minor}";

    public static SchemaVersion CurrentScenario { get; } = new(2, 0);
    public static SchemaVersion CurrentExperiment { get; } = new(2, 0);
    public static SchemaVersion CurrentWorkspace { get; } = new(1, 0);
}
