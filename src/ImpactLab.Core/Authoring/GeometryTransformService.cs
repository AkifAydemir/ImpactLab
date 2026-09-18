using ImpactLab.Core.Geometry;
using ImpactLab.Core.Geometry.Imported;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Authoring;

public static class GeometryTransformService
{
    public static GeometrySpec Translate(GeometrySpec geometry, Vec3 delta) =>
        geometry switch
        {
            RectangularPlateSpec p => new RectangularPlateSpec(
                p.Name,
                p.Center + delta,
                p.Width,
                p.Height,
                p.Thickness
            ),
            BoxSpec b => new BoxSpec(b.Name, b.Center + delta, b.Size),
            CylinderSpec c => new CylinderSpec(c.Name, c.Center + delta, c.Radius, c.Length),
            TubeSpec t => new TubeSpec(
                t.Name,
                t.Center + delta,
                t.OuterRadius,
                t.WallThickness,
                t.Length
            ),
            AngleProfileSpec a => new AngleProfileSpec(
                a.Name,
                a.Center + delta,
                a.LegX,
                a.LegY,
                a.Thickness,
                a.Length
            ),
            SphereSpec s => new SphereSpec(s.Name, s.Center + delta, s.Radius),
            TriangleMeshSpec m => new TriangleMeshSpec(m.Name, m.Asset, m.Translation + delta),
            _ => throw new NotSupportedException(
                $"Translation not implemented for {geometry.GetType().Name}."
            ),
        };
}
