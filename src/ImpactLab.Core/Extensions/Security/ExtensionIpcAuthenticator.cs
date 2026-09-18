using System.Security.Cryptography;
using System.Text;

namespace ImpactLab.Core.Extensions.Security;

public sealed class ExtensionIpcAuthenticator
{
    private readonly byte[] _key;
    private readonly TimeSpan _maxClockSkew;

    public ExtensionIpcAuthenticator(ReadOnlySpan<byte> key, TimeSpan? maxClockSkew = null)
    {
        if (key.Length < 32)
            throw new ArgumentException("IPC key must contain at least 256 bits.", nameof(key));
        _key = key.ToArray();
        _maxClockSkew = maxClockSkew ?? TimeSpan.FromMinutes(2);
    }

    public AuthenticatedIpcEnvelope Sign(
        string extensionId,
        string sessionId,
        string correlationId,
        ExtensionIpcMessageKind kind,
        IpcCapability capability,
        string payloadJson,
        DateTimeOffset? timestamp = null,
        string? nonce = null
    )
    {
        var e = new AuthenticatedIpcEnvelope(
            AuthenticatedIpcEnvelope.CurrentProtocolVersion,
            extensionId,
            sessionId,
            correlationId,
            timestamp ?? DateTimeOffset.UtcNow,
            nonce ?? Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant(),
            kind,
            capability,
            payloadJson,
            string.Empty
        );
        return e with { MacBase64 = Convert.ToBase64String(Mac(e)) };
    }

    public bool Verify(
        AuthenticatedIpcEnvelope e,
        string extensionId,
        string sessionId,
        IpcAuthorizationPolicy policy,
        IpcReplayGuard replay,
        DateTimeOffset? now = null
    )
    {
        var utc = now ?? DateTimeOffset.UtcNow;
        if (
            e.ProtocolVersion != AuthenticatedIpcEnvelope.CurrentProtocolVersion
            || !string.Equals(e.ExtensionId, extensionId, StringComparison.Ordinal)
            || !string.Equals(e.SessionId, sessionId, StringComparison.Ordinal)
            || Math.Abs((utc - e.TimestampUtc).TotalSeconds) > _maxClockSkew.TotalSeconds
            || !policy.Allows(e.Capability)
        )
            return false;
        byte[] supplied;
        try
        {
            supplied = Convert.FromBase64String(e.MacBase64);
        }
        catch (FormatException)
        {
            return false;
        }
        var expected = Mac(e with { MacBase64 = string.Empty });
        if (
            supplied.Length != expected.Length
            || !CryptographicOperations.FixedTimeEquals(supplied, expected)
        )
            return false;
        return replay.TryAccept(e.Nonce, utc);
    }

    private byte[] Mac(AuthenticatedIpcEnvelope e)
    {
        using var h = new HMACSHA256(_key);
        return h.ComputeHash(Encoding.UTF8.GetBytes(Canonical(e)));
    }

    private static string Canonical(AuthenticatedIpcEnvelope e)
    {
        var payloadHash = Convert
            .ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(e.PayloadJson)))
            .ToLowerInvariant();
        return string.Join(
            "\n",
            e.ProtocolVersion,
            e.ExtensionId,
            e.SessionId,
            e.CorrelationId,
            e.TimestampUtc.UtcDateTime.ToString("O"),
            e.Nonce,
            (int)e.Kind,
            (int)e.Capability,
            payloadHash
        );
    }
}
