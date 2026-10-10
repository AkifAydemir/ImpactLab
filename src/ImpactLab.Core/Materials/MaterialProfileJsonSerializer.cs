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
        File.WriteAllText(path, Serialize(profile));

    public static string Serialize(MaterialProfile profile)
    {
        profile.Validate();
        return JsonSerializer.Serialize(profile, Options);
    }

    public static MaterialProfile Load(string path)
    {
        return Deserialize(File.ReadAllText(path));
    }

    public static MaterialProfile Deserialize(string text)
    {
        var profile =
            JsonSerializer.Deserialize<MaterialProfile>(text, Options)
            ?? throw new InvalidDataException("Material profile file is empty.");
        profile.Validate();
        return profile;
    }
}
