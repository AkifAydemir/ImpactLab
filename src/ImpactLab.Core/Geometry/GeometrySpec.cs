using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry;

public abstract class GeometrySpec
{
    protected GeometrySpec(string name, Vec3 center)
    {
        Name = name;
        Center = center;
    }

    public string Name { get; }
    public Vec3 Center { get; }
    public abstract GeometryKind Kind { get; }
    public abstract BoundingBox3 Bounds { get; }
    public abstract bool Contains(in Vec3 point);
}
