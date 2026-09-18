using System.Text.Json;
using System.Text.Json.Serialization;

namespace ImpactLab.Core.Experiments.Workers;

public static class WorkerJsonProtocol
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    public static T Deserialize<T>(string text) =>
        JsonSerializer.Deserialize<T>(text, Options)
        ?? throw new WorkerProtocolException("Invalid worker JSON.");

    public static string Envelope<T>(WorkerMessageKind kind, string correlationId, T payload)
    {
        var element = JsonSerializer.SerializeToElement(payload, Options);
        return Serialize(
            new WorkerProtocolEnvelope(
                ExperimentWorkerProtocol.Version,
                kind,
                correlationId,
                element,
                DateTimeOffset.UtcNow
            )
        );
    }

    public static T Payload<T>(WorkerProtocolEnvelope envelope)
    {
        ExperimentWorkerProtocol.ValidateVersion(envelope.ProtocolVersion);
        return envelope.Payload.Deserialize<T>(Options)
            ?? throw new WorkerProtocolException($"Missing {envelope.Kind} payload.");
    }
}
