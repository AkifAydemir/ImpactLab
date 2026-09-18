namespace ImpactLab.Core.Finalization;

public sealed record ReadinessFinding(
    string Id,
    ReadinessFindingSeverity Severity,
    string Area,
    string Message,
    string? Evidence
);
