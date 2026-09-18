namespace ImpactLab.Core.Extensions.Security;

[Flags]
public enum ExtensionPermission
{
    None = 0,
    ReadProject = 1,
    WriteProject = 2,
    ReadFiles = 4,
    WriteFiles = 8,
    Network = 16,
    StartProcess = 32,
    NativeCode = 64,
    UnsafeCode = 128,
    All = int.MaxValue,
}
