namespace ImpactLab.Core.Extensions.Security;

public sealed record AuthenticatedIpcEnvelope(
    int ProtocolVersion,
    string ExtensionId,
    string SessionId,
    string CorrelationId,
    DateTimeOffset TimestampUtc,
    string Nonce,
    ExtensionIpcMessageKind Kind,
    IpcCapability Capability,
    string PayloadJson,
    string MacBase64
)
{
    public const int CurrentProtocolVersion = 1;
}
