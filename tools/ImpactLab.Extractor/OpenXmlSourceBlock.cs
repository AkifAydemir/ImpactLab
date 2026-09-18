namespace ImpactLab.Extractor;

public sealed record OpenXmlSourceBlock(
    string Path,
    string Content,
    int DocumentOrder,
    string HeadingText
);
