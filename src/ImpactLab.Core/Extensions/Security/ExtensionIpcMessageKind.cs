namespace ImpactLab.Core.Extensions.Security;

public enum ExtensionIpcMessageKind
{
    Hello,
    Request,
    Response,
    Heartbeat,
    Shutdown,
    Error,
}
