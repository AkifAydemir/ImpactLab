namespace ImpactLab.Core.Extensions.Security;

[Flags]
public enum IpcCapability
{
    None = 0,
    ReadScenario = 1,
    ReadResults = 2,
    WriteDerivedResults = 4,
    AddReportSection = 8,
    RegisterImporter = 16,
    RegisterBackend = 32,
}
