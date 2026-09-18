using System.Text;

namespace ImpactLab.Extractor;

public sealed class RepositoryWriter
{
    public RepositoryWriter(string root, bool requireEmpty = true)
    {
        Root = Path.GetFullPath(root);
        if (
            requireEmpty
            && Directory.Exists(Root)
            && Directory.EnumerateFileSystemEntries(Root).Any()
        )
            throw new InvalidOperationException($"Extraction output must be empty: {Root}");
        Directory.CreateDirectory(Root);
    }

    public string Root { get; }

    public string Write(string path, string content)
    {
        var relative = CanonicalPathPolicy.Normalize(path);
        var target = Path.GetFullPath(
            Path.Combine(Root, relative.Replace('/', Path.DirectorySeparatorChar))
        );
        var rootPrefix = Root.EndsWith(Path.DirectorySeparatorChar)
            ? Root
            : Root + Path.DirectorySeparatorChar;
        if (!target.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Canonical path escapes repository root: {path}");
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.WriteAllText(target, content, new UTF8Encoding(false));
        return target;
    }

    public void Write(IEnumerable<CanonicalSourceBlock> blocks)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var block in blocks.OrderBy(x => x.Path, StringComparer.Ordinal))
        {
            var normalized = CanonicalPathPolicy.Normalize(block.Path);
            if (!seen.Add(normalized))
                throw new InvalidOperationException(
                    $"Duplicate active canonical path: {normalized}"
                );
            Write(normalized, block.Content);
        }
    }
}
