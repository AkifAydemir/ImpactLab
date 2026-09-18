namespace ImpactLab.Core.Geometry.Repair;

public sealed record MeshRepairIssue(string Code, string Message, int Count, string Severity);
