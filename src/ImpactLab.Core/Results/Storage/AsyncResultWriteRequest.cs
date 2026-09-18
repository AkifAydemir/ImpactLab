namespace ImpactLab.Core.Results.Storage;

public sealed record AsyncResultWriteRequest(
    string FieldId,
    int FrameIndex,
    double TimeSeconds,
    double[] Values,
    int Components
);
