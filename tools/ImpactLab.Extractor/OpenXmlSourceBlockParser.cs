using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ImpactLab.Extractor;

public static class OpenXmlSourceBlockParser
{
    private static readonly XNamespace W =
        "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
    private static readonly Regex SourceHeading = new(@"^7\.\d+\s+/(.+)$", RegexOptions.Compiled);

    public static IReadOnlyList<OpenXmlSourceBlock> Parse(string docx)
    {
        using var archive = ZipFile.OpenRead(docx);
        var entry =
            archive.GetEntry("word/document.xml")
            ?? throw new InvalidDataException("document.xml missing");
        using var stream = entry.Open();
        var xml = XDocument.Load(stream);
        var paragraphs = xml.Descendants(W + "p").Select(ReadParagraphText).ToArray();
        var snapshotStart = Array.FindIndex(
            paragraphs,
            p => p.Trim() == "7. Current Canonical Source Snapshot"
        );
        if (snapshotStart < 0)
            throw new InvalidDataException("Current Canonical Source Snapshot heading missing.");
        var snapshotEnd = Array.FindIndex(
            paragraphs,
            snapshotStart + 1,
            p => p.TrimStart().StartsWith("8. Extraction Contract", StringComparison.Ordinal)
        );
        if (snapshotEnd < 0)
            throw new InvalidDataException(
                "Extraction Contract heading missing after current snapshot."
            );
        var blocks = new List<OpenXmlSourceBlock>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = snapshotStart + 1; i < snapshotEnd - 1; i++)
        {
            var heading = paragraphs[i].Trim();
            var match = SourceHeading.Match(heading);
            if (!match.Success)
                continue;
            var path = CanonicalPathPolicy.Normalize(match.Groups[1].Value);
            if (!seen.Add(path))
                throw new InvalidDataException($"Duplicate active canonical path: {path}");
            if (i + 1 >= snapshotEnd || SourceHeading.IsMatch(paragraphs[i + 1].Trim()))
                throw new InvalidDataException($"Missing source body after canonical path: {path}");
            blocks.Add(new(path, paragraphs[i + 1], i, heading));
        }
        if (blocks.Count == 0)
            throw new InvalidDataException(
                "Current Canonical Source Snapshot contains no source blocks."
            );
        return blocks;
    }

    private static string ReadParagraphText(XElement paragraph)
    {
        var text = new StringBuilder();
        foreach (var element in paragraph.Descendants())
        {
            if (element.Name == W + "t")
                text.Append(element.Value);
            else if (element.Name == W + "tab")
                text.Append('\t');
            else if (element.Name == W + "br" || element.Name == W + "cr")
                text.Append('\n');
        }
        return text.ToString();
    }
}
