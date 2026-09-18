namespace ImpactLab.Core.Parameters;

public sealed record ParameterValidationIssue(string Key, string Message, double? Value = null);
