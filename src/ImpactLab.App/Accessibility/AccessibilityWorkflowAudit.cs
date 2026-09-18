namespace ImpactLab.App.Accessibility;

public sealed record AccessibilityWorkflowFinding(string StepId, string Message, bool Blocking);

public static class AccessibilityWorkflowAudit
{
    public static IReadOnlyList<AccessibilityWorkflowFinding> Validate(
        IReadOnlyList<AccessibilityWorkflowStep> steps
    )
    {
        var f = new List<AccessibilityWorkflowFinding>();
        foreach (var s in steps)
        {
            if (string.IsNullOrWhiteSpace(s.AutomationName))
                f.Add(new(s.Id, "Automation name is missing.", s.Required));
            if (s.Required && string.IsNullOrWhiteSpace(s.Name))
                f.Add(new(s.Id, "Required workflow step has no accessible name.", true));
        }
        if (
            steps.Select(x => x.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count()
            != steps.Count
        )
            f.Add(new("catalog", "Duplicate accessibility workflow ids.", true));
        return f;
    }
}
