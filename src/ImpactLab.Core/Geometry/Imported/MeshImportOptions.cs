namespace ImpactLab.Core.Geometry.Imported;

public sealed record MeshImportOptions(
    LengthUnit SourceUnit = LengthUnit.Meter,
    bool CenterAtOrigin = false,
    bool WeldVertices = false,
    double WeldToleranceMeters = 1e-8
)
{
    public double ScaleToMeters =>
        SourceUnit switch
        {
            LengthUnit.Meter => 1.0,
            LengthUnit.Millimeter => 0.001,
            LengthUnit.Centimeter => 0.01,
            LengthUnit.Inch => 0.0254,
            _ => 1.0,
        };
}
