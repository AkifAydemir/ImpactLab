namespace ImpactLab.Core.Extensions;

[Flags]
public enum ExtensionCapability
{
    None = 0,
    SimulationBackend = 1,
    MaterialProvider = 2,
    GeometryImporter = 4,
    ReportContributor = 8,
    VerificationProvider = 16,
    WorkspaceTool = 32,
}
