namespace ImpactLab.Core.Extensions.Security;

public sealed record ExtensionHostResponse(
    bool Success,
    string PayloadJson = "{}",
    string? Error = null
);
