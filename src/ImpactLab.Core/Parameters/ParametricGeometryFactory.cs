using ImpactLab.Core.Geometry;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Parameters;

public static class ParametricGeometryFactory
{
    public static GeometrySpec Create(GeometryKind kind, string name, ParameterSet values)
    {
        if (kind == GeometryKind.ImportedMesh)
            throw new InvalidOperationException(
                "ImportedMesh requires MeshImportService and cannot be created from parametric values alone."
            );
        var schema = GeometryParameterSchemas.For(kind);
        values.ApplySchema(schema);
        var issues = schema.Validate(values);
        if (issues.Count != 0)
            throw new InvalidOperationException(string.Join("; ", issues.Select(x => x.Message)));
        var center = new Vec3(values.Get("centerX"), values.Get("centerY"), values.Get("centerZ"));
        return kind switch
        {
            GeometryKind.Plate => new RectangularPlateSpec(
                name,
                center,
                values.Get("width"),
                values.Get("height"),
                values.Get("thickness")
            ),
            GeometryKind.Box => new BoxSpec(
                name,
                center,
                new Vec3(values.Get("sizeX"), values.Get("sizeY"), values.Get("sizeZ"))
            ),
            GeometryKind.Cylinder => new CylinderSpec(
                name,
                center,
                values.Get("radius"),
                values.Get("length")
            ),
            GeometryKind.Tube => new TubeSpec(
                name,
                center,
                values.Get("outerRadius"),
                values.Get("wallThickness"),
                values.Get("length")
            ),
            GeometryKind.AngleProfile => new AngleProfileSpec(
                name,
                center,
                values.Get("legX"),
                values.Get("legY"),
                values.Get("thickness"),
                values.Get("length")
            ),
            GeometryKind.Sphere => new SphereSpec(name, center, values.Get("radius")),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }
}
