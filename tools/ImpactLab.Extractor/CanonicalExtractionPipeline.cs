namespace ImpactLab.Extractor;

public static class CanonicalExtractionPipeline
{
    public static ExtractionReport Extract(string docx, string output)
    {
        var blocks = OpenXmlSourceBlockParser.Parse(docx);
        var canonical = blocks
            .Select(x => new CanonicalSourceBlock(
                CanonicalPathPolicy.Normalize(x.Path),
                x.Content,
                x.DocumentOrder
            ))
            .OrderBy(x => x.Path, StringComparer.Ordinal)
            .ToArray();
        var writer = new RepositoryWriter(output, requireEmpty: true);
        writer.Write(canonical);
        var manifest = RepositoryManifestCalculator.Calculate(output);
        return new(
            blocks.Count,
            canonical.Length,
            Path.GetFullPath(output),
            canonical.Select(x => x.Path).ToArray(),
            manifest
        );
    }
}
