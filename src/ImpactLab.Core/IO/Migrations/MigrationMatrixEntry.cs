namespace ImpactLab.Core.IO.Migrations;

public sealed record MigrationMatrixEntry(
    string DocumentType,
    int FromVersion,
    int ToVersion,
    string MigrationId,
    bool RoundTripFixture,
    bool GoldenFixture
);
