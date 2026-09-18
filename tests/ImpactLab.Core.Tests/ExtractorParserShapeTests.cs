using System.IO.Compression;
using System.Xml.Linq;
using ImpactLab.Extractor;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class ExtractorParserShapeTests
{
    [Fact]
    public void ReadsOnlyCurrentSnapshotSection()
    {
        var path = CreateDocx([
            "4. Compact Immutable History",
            "7.99 /history/fake.cs",
            "historical text is not source",
            "7. Current Canonical Source Snapshot",
            "7.1 /src/a.cs",
            "namespace A;",
            "8. Extraction Contract",
            "after snapshot",
        ]);
        try
        {
            var blocks = OpenXmlSourceBlockParser.Parse(path);
            var block = Assert.Single(blocks);
            Assert.Equal("src/a.cs", block.Path);
            Assert.Equal("namespace A;", block.Content);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void RejectsDuplicateActivePathsCaseInsensitively()
    {
        var path = CreateDocx([
            "7. Current Canonical Source Snapshot",
            "7.1 /src/a.cs",
            "one",
            "7.2 /SRC/A.cs",
            "two",
            "8. Extraction Contract",
        ]);
        try
        {
            Assert.Throws<InvalidDataException>(() => OpenXmlSourceBlockParser.Parse(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void PreservesManualLineBreaksAndTabsInSourceBody()
    {
        var path = CreateDocxWithSourceBody("line1\nline2\tvalue");
        try
        {
            var block = Assert.Single(OpenXmlSourceBlockParser.Parse(path));
            Assert.Equal("line1\nline2\tvalue", block.Content);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string CreateDocxWithSourceBody(string source)
    {
        XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        var run = new XElement(w + "r");
        var parts = source.Split('\n');
        for (var i = 0; i < parts.Length; i++)
        {
            if (i > 0)
                run.Add(new XElement(w + "br"));
            var tabParts = parts[i].Split('\t');
            for (var j = 0; j < tabParts.Length; j++)
            {
                if (j > 0)
                    run.Add(new XElement(w + "tab"));
                run.Add(new XElement(w + "t", tabParts[j]));
            }
        }
        var document = new XDocument(
            new XElement(
                w + "document",
                new XAttribute(XNamespace.Xmlns + "w", w),
                new XElement(
                    w + "body",
                    new XElement(
                        w + "p",
                        new XElement(
                            w + "r",
                            new XElement(w + "t", "7. Current Canonical Source Snapshot")
                        )
                    ),
                    new XElement(
                        w + "p",
                        new XElement(w + "r", new XElement(w + "t", "7.1 /src/a.cs"))
                    ),
                    new XElement(w + "p", run),
                    new XElement(
                        w + "p",
                        new XElement(w + "r", new XElement(w + "t", "8. Extraction Contract"))
                    )
                )
            )
        );
        var path = Path.Combine(Path.GetTempPath(), $"impactlab-extractor-{Guid.NewGuid():N}.docx");
        using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
        var entry = archive.CreateEntry("word/document.xml");
        using var stream = entry.Open();
        document.Save(stream);
        return path;
    }

    private static string CreateDocx(IReadOnlyList<string> paragraphs)
    {
        XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        var body = new XElement(
            w + "body",
            paragraphs.Select(text => new XElement(
                w + "p",
                new XElement(w + "r", new XElement(w + "t", text))
            ))
        );
        var document = new XDocument(
            new XElement(w + "document", new XAttribute(XNamespace.Xmlns + "w", w), body)
        );
        var path = Path.Combine(Path.GetTempPath(), $"impactlab-extractor-{Guid.NewGuid():N}.docx");
        using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
        var entry = archive.CreateEntry("word/document.xml");
        using var stream = entry.Open();
        document.Save(stream);
        return path;
    }
}
