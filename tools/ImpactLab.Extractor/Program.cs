namespace ImpactLab.Extractor;

public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine(
                "usage: ImpactLab.Extractor <canonical.docx> <output-dir> [expected-aggregate-sha256]"
            );
            return 2;
        }
        var report = CanonicalExtractionPipeline.Extract(args[0], args[1]);
        Console.WriteLine(
            $"Extracted {report.UniqueFiles} canonical files from {report.SourceBlocks} source blocks."
        );
        Console.WriteLine($"Aggregate SHA-256: {report.RepositoryManifest.AggregateSha256}");
        if (
            args.Length >= 3
            && !report.RepositoryManifest.AggregateSha256.Equals(
                args[2],
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            Console.Error.WriteLine($"Manifest mismatch: expected {args[2]}.");
            return 3;
        }
        return 0;
    }
}
