using System.Text.Json;
using System.Text.Json.Serialization;

namespace ImpactLab.Core.Materials;

public static class MaterialProfileJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static void Save(string path, MaterialProfile profile) =>
        File.WriteAllText(path, JsonSerializer.Serialize(profile, Options));

    public static MaterialProfile Load(string path)
    {
        var profile =
            JsonSerializer.Deserialize<MaterialProfile>(File.ReadAllText(path), Options)
            ?? throw new InvalidDataException("Material profile file is empty.");
        profile.Validate();
        return profile;
    }
}
