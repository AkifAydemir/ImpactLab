namespace ImpactLab.Core.IO;

public sealed class WorkspaceDocument
{
    public string Name { get; set; } = "ImpactLab workspace";
    public string? ScenarioPath { get; set; }
    public List<string> ExperimentPaths { get; } = [];
    public List<string> VerificationSuitePaths { get; } = [];
    public List<string> MaterialProfilePaths { get; } = [];
    public string SelectedBackendId { get; set; } = "advanced-lattice-v1";
    public string? RunArchiveDirectory { get; set; }
    public WorkspaceLayoutState Layout { get; set; } = new();
    public List<string> RecentFiles { get; } = [];
    public DateTimeOffset ModifiedUtc { get; set; } = DateTimeOffset.UtcNow;
}
