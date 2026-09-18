namespace ImpactLab.Core.Extensions.Security;

public interface IExtensionSignatureVerifier
{
    ExtensionTrustDecision Verify(string packagePath, ExtensionManifest manifest);
}
