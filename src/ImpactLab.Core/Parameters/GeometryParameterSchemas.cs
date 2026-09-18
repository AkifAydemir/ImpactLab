using ImpactLab.Core.Geometry;

namespace ImpactLab.Core.Parameters;

public static class GeometryParameterSchemas
{
    private static readonly IReadOnlyDictionary<GeometryKind, ParameterSchema> Schemas =
        Enum.GetValues<GeometryKind>().ToDictionary(x => x, Create);

    public static ParameterSchema For(GeometryKind kind) => Schemas[kind];

    private static ParameterSchema Create(GeometryKind kind)
    {
        var p = new List<ParameterDefinition>
        {
            D("centerX", "Center X", "m", 0, -10, 10, 0.001),
            D("centerY", "Center Y", "m", 0, -10, 10, 0.001),
            D("centerZ", "Center Z", "m", 0, -10, 10, 0.001),
        };
        switch (kind)
        {
            case GeometryKind.Plate:
                p.AddRange([
                    D("width", "Width", "m", 0.30, 0.001, 20, 0.001),
                    D("height", "Height", "m", 0.30, 0.001, 20, 0.001),
                    D("thickness", "Thickness", "m", 0.05, 0.0005, 5, 0.0005),
                ]);
                break;
            case GeometryKind.Box:
                p.AddRange([
                    D("sizeX", "Size X", "m", 0.24, 0.001, 20, 0.001),
                    D("sizeY", "Size Y", "m", 0.24, 0.001, 20, 0.001),
                    D("sizeZ", "Size Z", "m", 0.12, 0.001, 20, 0.001),
                ]);
                break;
            case GeometryKind.Cylinder:
                p.AddRange([
                    D("radius", "Radius", "m", 0.12, 0.0005, 10, 0.0005),
                    D("length", "Length", "m", 0.20, 0.001, 20, 0.001),
                ]);
                break;
            case GeometryKind.Tube:
                p.AddRange([
                    D("outerRadius", "Outer radius", "m", 0.12, 0.001, 10, 0.001),
                    D("wallThickness", "Wall thickness", "m", 0.035, 0.0005, 5, 0.0005),
                    D("length", "Length", "m", 0.20, 0.001, 20, 0.001),
                ]);
                break;
            case GeometryKind.AngleProfile:
                p.AddRange([
                    D("legX", "Leg X", "m", 0.22, 0.001, 10, 0.001),
                    D("legY", "Leg Y", "m", 0.22, 0.001, 10, 0.001),
                    D("thickness", "Thickness", "m", 0.05, 0.0005, 5, 0.0005),
                    D("length", "Length", "m", 0.22, 0.001, 20, 0.001),
                ]);
                break;
            case GeometryKind.Sphere:
                p.Add(D("radius", "Radius", "m", 0.12, 0.0005, 10, 0.0005));
                break;
            case GeometryKind.ImportedMesh:
                break;
        }
        return new ParameterSchema($"geometry:{kind}", kind.ToString(), p);
    }

    private static ParameterDefinition D(
        string key,
        string name,
        string unit,
        double value,
        double min,
        double max,
        double step
    ) => new(key, name, unit, value, min, max, step);
}
