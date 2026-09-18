namespace ImpactLab.Core.Extensions;

public sealed record IsolatedExtensionResponse(
    bool Success,
    string PayloadJson,
    string? Error = null
);
