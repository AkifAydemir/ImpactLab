namespace ImpactLab.Core.Reporting;

public sealed record ReportGoldenVerificationResult(bool Passed, IReadOnlyList<string> Findings);
