namespace ImpactLab.Core.Geometry.Exchange;

public sealed record CadExchangeDescriptor(
    string Id,
    string DisplayName,
    CadExchangeCapability Capabilities,
    IReadOnlyList<string> Extensions,
    string? Version = null
);
