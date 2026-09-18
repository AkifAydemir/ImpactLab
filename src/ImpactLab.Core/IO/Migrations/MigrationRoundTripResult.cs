namespace ImpactLab.Core.IO.Migrations;

public sealed record MigrationRoundTripResult(
    string DocumentType,
    int SourceVersion,
    int TargetVersion,
    bool Success,
    string? Difference,
    string? Error
);
