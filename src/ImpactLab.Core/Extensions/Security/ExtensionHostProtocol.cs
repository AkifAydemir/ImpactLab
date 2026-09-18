using System.Text.Json;
using System.Text.Json.Serialization;

namespace ImpactLab.Core.Extensions.Security;

public static class ExtensionHostProtocol
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Json);

    public static T Deserialize<T>(string text) =>
        JsonSerializer.Deserialize<T>(text, Json)
        ?? throw new InvalidDataException($"Invalid extension-host {typeof(T).Name} payload.");

    public static async Task WriteLineAsync(
        TextWriter writer,
        object value,
        CancellationToken ct = default
    )
    {
        ct.ThrowIfCancellationRequested();
        await writer.WriteLineAsync(Serialize(value));
        await writer.FlushAsync(ct);
    }

    public static async Task<T> ReadLineAsync<T>(TextReader reader, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var line =
            await reader.ReadLineAsync(ct)
            ?? throw new EndOfStreamException("Extension host pipe closed.");
        return Deserialize<T>(line);
    }
}
