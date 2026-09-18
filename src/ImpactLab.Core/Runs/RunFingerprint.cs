using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.Core.Runs;

public static class RunFingerprint
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string Compute(ScenarioDefinition scenario)
    {
        var json = JsonSerializer.Serialize(scenario, Options);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
