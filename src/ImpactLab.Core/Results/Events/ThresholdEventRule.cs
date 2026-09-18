namespace ImpactLab.Core.Results.Events;

public sealed record ThresholdEventRule(
    string Id,
    string FieldId,
    double Threshold,
    bool GreaterThan = true,
    ResultEventSeverity Severity = ResultEventSeverity.Warning
);
