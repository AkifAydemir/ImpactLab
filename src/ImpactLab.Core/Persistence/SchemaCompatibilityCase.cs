namespace ImpactLab.Core.Persistence;

public sealed record SchemaCompatibilityCase(
    string Id,
    string Kind,
    int FromVersion,
    int ToVersion,
    string FixturePath
);
