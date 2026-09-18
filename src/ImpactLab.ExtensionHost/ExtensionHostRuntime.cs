using ImpactLab.Core.Extensions;
using ImpactLab.Core.Extensions.Security;

namespace ImpactLab.ExtensionHost;

public sealed class ExtensionHostRuntime
{
    public async Task<int> RunAsync(TextReader input, TextWriter output, CancellationToken ct)
    {
        var bootstrap = await ExtensionHostProtocol.ReadLineAsync<ExtensionHostBootstrap>(
            input,
            ct
        );
        bootstrap.Validate();
        var auth = new ExtensionIpcAuthenticator(bootstrap.DecodeKey());
        var policy = new IpcAuthorizationPolicy(
            bootstrap.AllowedCapabilities,
            bootstrap.ExtensionId
        );
        var replay = new IpcReplayGuard();
        using var extension = new ExtensionHostAssemblyRuntime(
            bootstrap.EntryAssemblyPath,
            bootstrap.EntryType,
            bootstrap.GrantedPermissions.HasFlag(ExtensionPermission.NativeCode)
        );
        var helloPayload = ExtensionHostProtocol.Serialize(
            new
            {
                pid = Environment.ProcessId,
                extension = bootstrap.ExtensionId,
                isolated = true,
            }
        );
        await ExtensionHostProtocol.WriteLineAsync(
            output,
            auth.Sign(
                bootstrap.ExtensionId,
                bootstrap.SessionId,
                Guid.NewGuid().ToString("N"),
                ExtensionIpcMessageKind.Hello,
                IpcCapability.None,
                helloPayload
            ),
            ct
        );
        while (!ct.IsCancellationRequested)
        {
            AuthenticatedIpcEnvelope envelope;
            try
            {
                envelope = await ExtensionHostProtocol.ReadLineAsync<AuthenticatedIpcEnvelope>(
                    input,
                    ct
                );
            }
            catch (EndOfStreamException)
            {
                return 0;
            }
            if (
                envelope.Kind != ExtensionIpcMessageKind.Request
                || !auth.Verify(
                    envelope,
                    bootstrap.ExtensionId,
                    bootstrap.SessionId,
                    policy,
                    replay
                )
            )
            {
                await Reply(
                    false,
                    "{}",
                    "Unauthorized or replayed IPC envelope.",
                    ExtensionIpcMessageKind.Error
                );
                continue;
            }
            var req = ExtensionHostProtocol.Deserialize<ExtensionHostRequest>(envelope.PayloadJson);
            if (req.Operation.Equals("shutdown", StringComparison.OrdinalIgnoreCase))
            {
                await Reply(true, "{}", null, ExtensionIpcMessageKind.Response);
                return 0;
            }
            if (req.Operation.Equals("ping", StringComparison.OrdinalIgnoreCase))
            {
                await Reply(
                    true,
                    ExtensionHostProtocol.Serialize(
                        new { utc = DateTimeOffset.UtcNow, pid = Environment.ProcessId }
                    ),
                    null,
                    ExtensionIpcMessageKind.Response
                );
                continue;
            }
            if (req.Operation.Equals("describe", StringComparison.OrdinalIgnoreCase))
            {
                await Reply(
                    true,
                    ExtensionHostProtocol.Serialize(extension.Manifest),
                    null,
                    ExtensionIpcMessageKind.Response
                );
                continue;
            }
            if (req.Operation.Equals("invoke", StringComparison.OrdinalIgnoreCase))
            {
                var result = await extension.InvokeAsync(
                    new(req.Operation, req.ArgumentsJson, envelope.Capability),
                    ct
                );
                await Reply(
                    result.Success,
                    result.PayloadJson,
                    result.Error,
                    result.Success
                        ? ExtensionIpcMessageKind.Response
                        : ExtensionIpcMessageKind.Error
                );
                continue;
            }
            await Reply(
                false,
                "{}",
                $"Unknown extension-host operation: {req.Operation}",
                ExtensionIpcMessageKind.Error
            );
            async Task Reply(
                bool success,
                string payload,
                string? error,
                ExtensionIpcMessageKind kind
            )
            {
                var body = ExtensionHostProtocol.Serialize(
                    new ExtensionHostResponse(success, payload, error)
                );
                await ExtensionHostProtocol.WriteLineAsync(
                    output,
                    auth.Sign(
                        bootstrap.ExtensionId,
                        bootstrap.SessionId,
                        envelope.CorrelationId,
                        kind,
                        envelope.Capability,
                        body
                    ),
                    ct
                );
            }
        }
        return 0;
    }
}
