using System.Text.Json;

namespace ImpactLab.Core.Experiments.Workers;

public sealed record WorkerProtocolEnvelope(
    int ProtocolVersion,
    WorkerMessageKind Kind,
    string CorrelationId,
    JsonElement Payload,
    DateTimeOffset TimestampUtc
);
