using System.Security.Cryptography;
using System.Text;

namespace ImpactLab.Extractor;

public static class RepositoryManifestCalculator
{
    public static RepositoryManifest Calculate(string root)
    {
        root = Path.GetFullPath(root);
        var entries = Directory
            .EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Select(path => Entry(root, path))
            .OrderBy(x => x.Path, StringComparer.Ordinal)
            .ToArray();
        var aggregateText = string.Join(
            "\n",
            entries.Select(x => $"{x.Path}\t{x.Bytes}\t{x.Sha256}")
        );
        var aggregate = Convert
            .ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(aggregateText)))
            .ToLowerInvariant();
        return new(entries, entries.Sum(x => x.Bytes), aggregate);
    }

    private static RepositoryManifestEntry Entry(string root, string path)
    {
        var bytes = File.ReadAllBytes(path);
        var relative = Path.GetRelativePath(root, path).Replace('\\', '/');
        return new(
            relative,
            bytes.LongLength,
            Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()
        );
    }
}
