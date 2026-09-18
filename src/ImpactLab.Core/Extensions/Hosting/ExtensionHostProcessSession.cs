using System.Diagnostics;
using System.Text.Json;
using ImpactLab.Core.Extensions.Sandbox;
using ImpactLab.Core.Extensions.Security;

namespace ImpactLab.Core.Extensions.Hosting;

public sealed class ExtensionHostProcessSession : IAsyncDisposable
{
    private readonly Process _process;
    private readonly ExtensionIpcAuthenticator _auth;
    private readonly IpcReplayGuard _replay = new();
    private readonly IpcAuthorizationPolicy _policy;
    private readonly IExtensionIsolationLease _lease;
    private readonly TimeSpan _timeout;
    public string ExtensionId { get; }
    public string SessionId { get; }
    public IReadOnlyList<string> IsolationDiagnostics => _lease.Diagnostics;

    internal ExtensionHostProcessSession(
        Process process,
        string extensionId,
        string sessionId,
        ExtensionIpcAuthenticator auth,
        IpcAuthorizationPolicy policy,
        IExtensionIsolationLease lease,
        TimeSpan timeout
    )
    {
        _process = process;
        ExtensionId = extensionId;
        SessionId = sessionId;
        _auth = auth;
        _policy = policy;
        _lease = lease;
        _timeout = timeout;
    }

    public async Task<ExtensionHostResponse> RequestAsync(
        string operation,
        IpcCapability capability,
        string argumentsJson = "{}",
        CancellationToken ct = default
    )
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linked.CancelAfter(_timeout);
        var correlation = Guid.NewGuid().ToString("N");
        var payload = ExtensionHostProtocol.Serialize(
            new ExtensionHostRequest(operation, argumentsJson)
        );
        var envelope = _auth.Sign(
            ExtensionId,
            SessionId,
            correlation,
            ExtensionIpcMessageKind.Request,
            capability,
            payload
        );
        await ExtensionHostProtocol.WriteLineAsync(_process.StandardInput, envelope, linked.Token);
        var reply = await ExtensionHostProtocol.ReadLineAsync<AuthenticatedIpcEnvelope>(
            _process.StandardOutput,
            linked.Token
        );
        if (
            reply.CorrelationId != correlation
            || reply.Kind is not (ExtensionIpcMessageKind.Response or ExtensionIpcMessageKind.Error)
            || !_auth.Verify(reply, ExtensionId, SessionId, _policy, _replay)
        )
            throw new UnauthorizedAccessException("Invalid authenticated extension-host response.");
        return ExtensionHostProtocol.Deserialize<ExtensionHostResponse>(reply.PayloadJson);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (!_process.HasExited)
                await RequestAsync("shutdown", IpcCapability.None, "{}", CancellationToken.None);
        }
        catch { }
        if (!_process.HasExited)
        {
            _process.Kill(true);
            await _process.WaitForExitAsync();
        }
        _process.Dispose();
        _lease.Dispose();
    }
}
