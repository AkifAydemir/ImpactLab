namespace ImpactLab.Core.Diagnostics;

public sealed record ValidationIssue(
    ValidationSeverity Severity,
    string Code,
    string Message,
    string? EntityId = null
);
