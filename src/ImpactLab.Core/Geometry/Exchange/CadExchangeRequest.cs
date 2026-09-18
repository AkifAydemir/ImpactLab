namespace ImpactLab.Core.Geometry.Exchange;

public sealed record CadExchangeRequest(
    string SourcePath,
    string OutputMeshPath,
    string? ConverterExecutable = null,
    double ChordToleranceMeters = 0.0005,
    double AngularToleranceDegrees = 10.0
);
