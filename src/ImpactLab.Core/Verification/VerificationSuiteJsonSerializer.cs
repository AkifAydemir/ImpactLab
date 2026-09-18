using System.Text.Json;
using System.Text.Json.Serialization;

namespace ImpactLab.Core.Verification;

public static class VerificationSuiteJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static void Save(string path, VerificationSuite suite) =>
        File.WriteAllText(path, JsonSerializer.Serialize(suite, Options));

    public static VerificationSuite Load(string path) =>
        JsonSerializer.Deserialize<VerificationSuite>(File.ReadAllText(path), Options)
        ?? throw new InvalidDataException("Verification suite is empty.");
}
