using System.Diagnostics;
using System.Security.Cryptography;
using ImpactLab.Core.Extensions.Sandbox;
using ImpactLab.Core.Extensions.Security;

namespace ImpactLab.Core.Extensions.Hosting;

public sealed class ExtensionProcessLauncher
{
    private readonly IExtensionIsolationProvider _isolation;

    public ExtensionProcessLauncher(IExtensionIsolationProvider? isolation = null) =>
        _isolation = isolation ?? ExtensionIsolationProviderSelector.CreateDefault();

    public async Task<ExtensionHostProcessSession> LaunchAsync(
        ExtensionLoadPlan plan,
        ExtensionHostProcessOptions options,
        CancellationToken ct = default
    )
    {
        if (plan.Mode != ExtensionExecutionMode.IsolatedProcess)
            throw new InvalidOperationException("Load plan is not isolated-process mode.");
        var psi = new ProcessStartInfo(options.HostExecutable)
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = plan.Package.Directory,
        };
        var p =
            Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start ImpactLab.ExtensionHost.");
        IExtensionIsolationLease? lease = null;
        try
        {
            lease = _isolation.Attach(p, options.SandboxPolicy);
            var session = Guid.NewGuid().ToString("N");
            var key = RandomNumberGenerator.GetBytes(32);
            var bootstrap = new ExtensionHostBootstrap(
                AuthenticatedIpcEnvelope.CurrentProtocolVersion,
                session,
                plan.Package.Manifest.Id,
                plan.Package.Directory,
                plan.Package.EntryAssemblyPath,
                plan.Package.Manifest.EntryType,
                plan.Permissions.Granted,
                plan.IpcAuthorization.Allowed,
                Convert.ToBase64String(key),
                Path.GetFullPath(options.WorkspaceRoot),
                Path.GetFullPath(options.TempRoot)
            );
            bootstrap.Validate();
            await ExtensionHostProtocol.WriteLineAsync(p.StandardInput, bootstrap, ct);
            var auth = new ExtensionIpcAuthenticator(key);
            var hello = await ExtensionHostProtocol.ReadLineAsync<AuthenticatedIpcEnvelope>(
                p.StandardOutput,
                ct
            );
            var replay = new IpcReplayGuard();
            if (
                hello.Kind != ExtensionIpcMessageKind.Hello
                || !auth.Verify(
                    hello,
                    bootstrap.ExtensionId,
                    session,
                    plan.IpcAuthorization,
                    replay
                )
            )
                throw new UnauthorizedAccessException(
                    "Extension host hello authentication failed."
                );
            return new(
                p,
                bootstrap.ExtensionId,
                session,
                auth,
                plan.IpcAuthorization,
                lease,
                options.EffectiveRequestTimeout
            );
        }
        catch
        {
            lease?.Dispose();
            if (!p.HasExited)
                p.Kill(true);
            p.Dispose();
            throw;
        }
    }
}
