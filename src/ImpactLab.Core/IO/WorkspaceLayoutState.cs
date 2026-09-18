namespace ImpactLab.Core.IO;

public sealed class WorkspaceLayoutState
{
    public string SelectedTab { get; set; } = "Simulation";
    public double LeftPanelWidth { get; set; } = 220;
    public double RightPanelWidth { get; set; } = 320;
    public bool TimelineVisible { get; set; } = true;
    public bool InspectorVisible { get; set; } = true;
    public Dictionary<string, double> Splitters { get; } = new(StringComparer.OrdinalIgnoreCase);
}
