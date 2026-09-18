namespace ImpactLab.Core.Persistence;

public sealed record GoldenFixture(
    string Id,
    string Kind,
    int SchemaVersion,
    string RelativePath,
    string Sha256
);
