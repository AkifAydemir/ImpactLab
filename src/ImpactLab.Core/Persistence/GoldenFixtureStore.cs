using System.Security.Cryptography;

namespace ImpactLab.Core.Persistence;

public sealed class GoldenFixtureStore
{
    public GoldenFixtureStore(string root) => Root = root;

    public string Root { get; }

    public string Read(GoldenFixture f) => File.ReadAllText(Path.Combine(Root, f.RelativePath));

    public bool Verify(GoldenFixture f)
    {
        var bytes = File.ReadAllBytes(Path.Combine(Root, f.RelativePath));
        var hash = Convert.ToHexString(SHA256.HashData(bytes));
        return hash.Equals(f.Sha256, StringComparison.OrdinalIgnoreCase);
    }

    public string Hash(string relative) =>
        Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(Root, relative))));
}
