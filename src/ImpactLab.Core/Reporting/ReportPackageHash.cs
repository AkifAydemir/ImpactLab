using System.Security.Cryptography;

namespace ImpactLab.Core.Reporting;

public static class ReportPackageHash
{
    public static string Bytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    public static string File(string path) => Bytes(System.IO.File.ReadAllBytes(path));
}
