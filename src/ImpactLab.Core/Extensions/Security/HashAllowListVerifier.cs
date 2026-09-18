using System.Security.Cryptography;

namespace ImpactLab.Core.Extensions.Security;

public sealed class HashAllowListVerifier : IExtensionSignatureVerifier
{
    private readonly HashSet<string> _allowed;

    public HashAllowListVerifier(IEnumerable<string> sha256) =>
        _allowed = new(sha256, StringComparer.OrdinalIgnoreCase);

    public ExtensionTrustDecision Verify(string packagePath, ExtensionManifest m)
    {
        using var s = File.OpenRead(packagePath);
        var h = Convert.ToHexString(SHA256.HashData(s));
        var ok = _allowed.Contains(h);
        return new(
            ok ? ExtensionTrustLevel.TrustedPublisher : ExtensionTrustLevel.Untrusted,
            ok,
            ok ? "Package hash allow-listed." : "Package hash not allow-listed.",
            h
        );
    }
}
