using System.Text.Json;

namespace ImpactLab.Core.Reporting;

public static class EngineeringReportCanonicalJson
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    public static string Serialize(EngineeringReportDocument document) =>
        JsonSerializer.Serialize(document, Options) + "\n";

    public static EngineeringReportDocument Deserialize(string text) =>
        JsonSerializer.Deserialize<EngineeringReportDocument>(text, Options)
        ?? throw new InvalidDataException("Invalid engineering report JSON.");
}
