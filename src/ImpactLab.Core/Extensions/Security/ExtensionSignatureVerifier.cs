using System.Security.Cryptography;
using System.Text;

namespace ImpactLab.Core.Extensions.Security;

public static class ExtensionSignatureVerifier
{
    public static bool VerifySha256Rsa(byte[] payload, ExtensionSignatureEnvelope e, RSA publicKey)
    {
        var hash = Convert.ToHexString(SHA256.HashData(payload));
        if (!hash.Equals(e.PayloadSha256, StringComparison.OrdinalIgnoreCase))
            return false;
        return publicKey.VerifyData(
            payload,
            Convert.FromBase64String(e.SignatureBase64),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1
        );
    }
}
