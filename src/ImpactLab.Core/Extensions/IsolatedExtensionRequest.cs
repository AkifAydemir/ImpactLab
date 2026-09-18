using ImpactLab.Core.Extensions.Security;

namespace ImpactLab.Core.Extensions;

public sealed record IsolatedExtensionRequest(
    string Operation,
    string PayloadJson,
    IpcCapability Capability
);
