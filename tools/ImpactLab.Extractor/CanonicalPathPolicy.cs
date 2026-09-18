namespace ImpactLab.Extractor;

public static class CanonicalPathPolicy
{
    private static readonly char[] ForbiddenFileNameCharacters =
    [
        '<',
        '>',
        ':',
        '"',
        '|',
        '?',
        '*',
    ];

    public static string Normalize(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new InvalidOperationException("Canonical path cannot be empty.");
        var raw = path.Trim();
        if (
            raw.StartsWith('/')
            || raw.StartsWith('\\')
            || raw.StartsWith("//", StringComparison.Ordinal)
            || raw.StartsWith("\\\\", StringComparison.Ordinal)
            || RegexDrivePrefix(raw)
        )
            throw new InvalidOperationException($"Rooted canonical path is forbidden: {path}");
        var normalized = raw.Replace('\\', '/');
        var segments = normalized.Split('/');
        if (
            segments.Any(segment =>
                string.IsNullOrEmpty(segment)
                || segment is "." or ".."
                || segment.Any(c => char.IsControl(c) || ForbiddenFileNameCharacters.Contains(c))
            )
        )
            throw new InvalidOperationException($"Unsafe canonical path: {path}");
        return string.Join('/', segments);
    }

    public static bool IsSourcePath(string path)
    {
        var p = Normalize(path);
        return p.Contains('/')
            || p.EndsWith(".sln", StringComparison.OrdinalIgnoreCase)
            || p.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
            || p.StartsWith('.');
    }

    private static bool RegexDrivePrefix(string path) =>
        path.Length >= 2 && char.IsAsciiLetter(path[0]) && path[1] == ':';
}
